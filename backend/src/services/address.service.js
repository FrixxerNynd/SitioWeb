import prisma from "../config/db.js";

// ═══════════════════════════════════════════════════════════
//  SERVICIO - Lógica de negocio de las direcciones del usuario
// ═══════════════════════════════════════════════════════════

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
    const {
      pais,
      estado,
      ciudad,
      codigoPostal,
      colonia,
      calle,
      nExterior: nExteriorSinProcesar,
      nInterior: nInteriorSinProcesar,
    } = data;

    if (!pais || !estado || !ciudad || !codigoPostal || !colonia || !calle || !nExteriorSinProcesar) {
      throw new Error("Faltan campos obligatorios para registrar la dirección.");
    }

    // Prisma espera String para nExterior; nInterior es opcional
    // y se guarda como string vacío si no lo mandan (no null, no "0").
    const nExterior = String(nExteriorSinProcesar);
    const nInterior = nInteriorSinProcesar ? String(nInteriorSinProcesar) : "";

    return await prisma.userAddress.create({
      data: {
        userId,
        pais,
        estado,
        ciudad,
        codigoPostal,
        colonia,
        calle,
        nExterior,
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

    const {
      pais,
      estado,
      ciudad,
      codigoPostal,
      colonia,
      calle,
      nExterior,
      nInterior,
    } = data;

    return await prisma.userAddress.update({
      where: {
        id: addressId,
      },
      data: {
        pais: pais ?? direccionExiste.pais,
        estado: estado ?? direccionExiste.estado,
        ciudad: ciudad ?? direccionExiste.ciudad,
        codigoPostal: codigoPostal ?? direccionExiste.codigoPostal,
        colonia: colonia ?? direccionExiste.colonia,
        calle: calle ?? direccionExiste.calle,
        nExterior: nExterior !== undefined
          ? String(nExterior)
          : direccionExiste.nExterior,
        // undefined → no vino en el body, se conserva el valor anterior
        // "" o cualquier valor falsy → se guarda como string vacío
        // cualquier otro valor → se normaliza a string
        nInterior: nInterior === undefined
          ? direccionExiste.nInterior
          : (nInterior ? String(nInterior) : ""),
      },
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