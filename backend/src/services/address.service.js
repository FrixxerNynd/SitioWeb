import prisma from "../config/db.js";

// ═══════════════════════════════════════════════════════════
//  SERVICIO - Lógica de negocio de las direcciones del usuario
// ═══════════════════════════════════════════════════════════

const CAMPOS_OBLIGATORIOS = ["pais", "estado", "ciudad", "codigoPostal", "colonia", "calle", "nExterior"];

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

const limpiarNInterior = (valor) =>
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
    const nInterior = limpiarNInterior(data.nInterior);

    // Evitar registrar dos veces exactamente la misma dirección para el mismo usuario
    const duplicada = await prisma.userAddress.findFirst({
      where: {
        userId,
        calle: limpio.calle,
        nExterior: limpio.nExterior,
        nInterior,
        colonia: limpio.colonia,
        ciudad: limpio.ciudad,
        estado: limpio.estado,
        codigoPostal: limpio.codigoPostal,
        pais: limpio.pais,
      },
    });

    if (duplicada) {
      throw new Error("Ya tienes registrada una dirección idéntica.");
    }

    return await prisma.userAddress.create({
      data: {
        userId,
        ...limpio,
        nInterior,
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

    if (data.nInterior !== undefined) {
      actualizacion.nInterior = limpiarNInterior(data.nInterior);
    }

    // Cómo quedaría la dirección después de aplicar los cambios
    const resultante = { ...direccionExiste, ...actualizacion };

    // Evitar que, tras la edición, quede idéntica a otra dirección ya guardada
    const duplicada = await prisma.userAddress.findFirst({
      where: {
        id: { not: addressId },
        userId,
        calle: resultante.calle,
        nExterior: resultante.nExterior,
        nInterior: resultante.nInterior,
        colonia: resultante.colonia,
        ciudad: resultante.ciudad,
        estado: resultante.estado,
        codigoPostal: resultante.codigoPostal,
        pais: resultante.pais,
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