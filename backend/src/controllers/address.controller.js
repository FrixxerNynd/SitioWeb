import addressService from "../services/address.service.js";

const getUserId = (req) => {
  const id = parseInt(req.user?.id);
  if (isNaN(id)) throw new Error("Token inválido: userId no encontrado");
  return id;
};

// ═══════════════════════════════════════════════════════════
//  CONTROLADOR - Manejo de requests y responses HTTP
// ═══════════════════════════════════════════════════════════

class AddressController {
  //CRUD
  //CREATE ADDDRESS
  // POST /api/users/addresses
  async createAddress(req, res, next) {
    try {
      const result = await addressService.createAddress(getUserId(req), req.body);
      res.status(201).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  ///READ ADDDRES
  // GET /api/users/addresses
  async getAddresses(req, res, next) {
    try {
      const result = await addressService.getAddresses(getUserId(req));
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }
  // READ ADDRES BY ID
  // GET /api/users/addresses/:addressId
  async getAddressById(req, res, next) {
    try {
      const result = await addressService.getAddressById(
        getUserId(req),
        parseInt(req.params.addressId)
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  
  
  //UPDATE
  // PUT /api/users/addresses/:addressId
  async updateAddress(req, res, next) {
    try {
      const result = await addressService.updateAddress(
        getUserId(req),
        parseInt(req.params.addressId),
        req.body
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }

  // DELETE
  // DELETE /api/users/addresses/:addressId
  async deleteAddress(req, res, next) {
    try {
      const result = await addressService.deleteAddress(
        getUserId(req),
        parseInt(req.params.addressId)
      );
      res.status(200).json({ success: true, data: result });
    } catch (error) {
      next(error);
    }
  }
}

export default new AddressController();
