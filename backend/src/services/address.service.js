// backend/src/services/address.service.js

import prisma from "../config/db.js";

// ═══════════════════════════════════════════════════════════
//  SERVICIO - Lógica de negocio de las direcciones del usuario
// ═══════════════════════════════════════════════════════════

const CAMPOS_OBLIGATORIOS = ["country", "state", "city", "cp", "neighborhood", "street", "extNum"];

// Recorta espacios y revienta si algún campo obligatorio viene vacío/undefined/null
const limpiarYValidarObligatorios = (data, campos = CAMPOS_OBLIGATORIOS) => {
  const limpio = {};
  for (const campo of campos) {
    const valor = data[campo];
    if (valor === undefined || valor === null || String(valor).trim() === "") {
      throw new Error(`El campo "${campo}" es obligatorio y no puede estar vacío.`);
    }
    limpio[campo] = String(valor).trim();
  }
  return limpio;
};

const limpiarIntNum = (valor) =>
  valor !== undefined && valor !== null && String(valor).trim() !== ""
    ? String(valor).trim()
    : "";

class AddressService {
  // ─── READ - Listar todas las direcciones del usuario ───
  async getAddresses(userId) {
    return await prisma.userAddress.findMany({
      where: { userId },
      orderBy: { id: "asc" },
    });
  }

  // ─── READ - Obtener una dirección específica del usuario ───
  async getAddressById(userId, addressId) {
    const direccion = await prisma.userAddress.findFirst({
      where: { id: addressId, userId },
    });

    if (!direccion) throw new Error("La dirección no existe.");
    return direccion;
  }

  // ─── CREATE - Agregar una nueva dirección ───
  async createAddress(userId, data) {
    const limpio = limpiarYValidarObligatorios(data);
    const intNum = limpiarIntNum(data.intNum);

    // Evitar registrar dos veces exactamente la misma dirección para el mismo usuario
    const duplicada = await prisma.userAddress.findFirst({
      where: {
        userId,
        street: limpio.street,
        extNum: limpio.extNum,
        intNum,
        neighborhood: limpio.neighborhood,
        city: limpio.city,
        state: limpio.state,
        cp: limpio.cp,
        country: limpio.country,
      },
    });

    if (duplicada) {
      throw new Error("Ya tienes registrada una dirección idéntica.");
    }

    return await prisma.userAddress.create({
      data: {
        userId,
        street: limpio.street,
        extNum: limpio.extNum,
        intNum,
        neighborhood: limpio.neighborhood,
        city: limpio.city,
        state: limpio.state,
        cp: limpio.cp,
        country: limpio.country,
      },
    });
  }

  // ─── UPDATE - Modificar una dirección existente ───
  async updateAddress(userId, addressId, data) {
    const direccionExiste = await prisma.userAddress.findFirst({
      where: { id: addressId, userId },
    });

    if (!direccionExiste) throw new Error("La dirección no existe.");

    const actualizacion = {};

    // Solo se valida lo que realmente viene en el body; si viene, no puede ser vacío
    for (const campo of CAMPOS_OBLIGATORIOS) {
      if (data[campo] !== undefined) {
        const valor = String(data[campo]).trim();
        if (valor === "") {
          throw new Error(`El campo "${campo}" no puede quedar vacío.`);
        }
        actualizacion[campo] = valor;
      }
    }

    if (data.intNum !== undefined) {
      actualizacion.intNum = limpiarIntNum(data.intNum);
    }

    // Cómo quedaría la dirección después de aplicar los cambios
    const resultante = { ...direccionExiste, ...actualizacion };

    // Evitar que, tras la edición, quede idéntica a otra dirección ya guardada
    const duplicada = await prisma.userAddress.findFirst({
      where: {
        id: { not: addressId },
        userId,
        street: resultante.street,
        extNum: resultante.extNum,
        intNum: resultante.intNum,
        neighborhood: resultante.neighborhood,
        city: resultante.city,
        state: resultante.state,
        cp: resultante.cp,
        country: resultante.country,
      },
    });

    if (duplicada) {
      throw new Error("Ya existe otra dirección idéntica registrada.");
    }

    return await prisma.userAddress.update({
      where: { id: addressId },
      data: actualizacion,
    });
  }

  // ─── DELETE - Eliminar una dirección ───
  async deleteAddress(userId, addressId) {
    const existing = await prisma.userAddress.findFirst({
      where: { id: addressId, userId },
    });

    if (!existing) throw new Error("La dirección no existe.");

    await prisma.userAddress.delete({
      where: { id: addressId },
    });

    return { id: addressId, deleted: true };
  }
}

export default new AddressService();