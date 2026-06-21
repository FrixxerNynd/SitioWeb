import prisma from '../config/db.js';

class PercentageController {

  async getPorcentajes(req, res, next) {
    try {
      const data = await prisma.percentages.findMany({
        orderBy: { id_categoria: 'asc' },
      });
      res.status(200).json({ success: true, data });
    } catch (error) {
      next(error);
    }
  }

  async getPorcentajePorCategoria(req, res, next) {
    try {
      const { id_categoria } = req.params;
      const data = await prisma.percentages.findUnique({
        where: { id_categoria },
      });
      if (!data) {
        return res.status(404).json({ success: false, message: 'Porcentaje no encontrado para esa categoría' });
      }
      res.status(200).json({ success: true, data });
    } catch (error) {
      next(error);
    }
  }

  async crearOActualizarPorcentaje(req, res, next) {
    try {
      const { id_categoria, nombre_categoria, porcentaje } = req.body;
      if (!id_categoria || porcentaje === undefined) {
        return res.status(400).json({
          success: false,
          message: 'id_categoria y porcentaje son requeridos',
        });
      }
      const pct = parseFloat(porcentaje);
      if (isNaN(pct) || pct < 0) {
        return res.status(400).json({
          success: false,
          message: 'porcentaje debe ser un número válido mayor o igual a 0',
        });
      }
      const data = await prisma.percentages.upsert({
        where: { id_categoria },
        update: { nombre_categoria: nombre_categoria ?? '', porcentaje: pct },
        create: { id_categoria, nombre_categoria: nombre_categoria ?? '', porcentaje: pct },
      });
      res.status(200).json({ success: true, data });
    } catch (error) {
      next(error);
    }
  }

  async eliminarPorcentaje(req, res, next) {
    try {
      const { id } = req.params;
      await prisma.percentages.delete({ where: { id: parseInt(id) } });
      res.status(200).json({ success: true, message: 'Porcentaje eliminado' });
    } catch (error) {
      if (error.code === 'P2025') {
        return res.status(404).json({ success: false, message: 'Porcentaje no encontrado' });
      }
      next(error);
    }
  }
}

export default new PercentageController();
