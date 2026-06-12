// =====================================================================================
// DTO RESPONSE ADM PRODUCTO LEGACY - AdmProductoResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para respuestas de API de productos legacy desde adCABS2016 (tabla admProductos).
// Incluye campos principales de la tabla legacy con FKs a monedas y unidades.
//
// PROPÓSITO:
// - Comunicación API-Frontend para productos legacy
// - Exponer datos completos de admProductos
// - Incluir precios, costos, impuestos y clasificaciones
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para productos legacy de admProductos (adCABS2016)
    /// Contiene información completa de productos del sistema Adminpaq
    /// </summary>
    public class AdmProductoResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // IDENTIFICACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del producto en el sistema legacy
        /// </summary>
        public int IdProducto { get; set; }

        /// <summary>
        /// Código del producto (ej: "PROD001", "ART-001")
        /// </summary>
        public string CodigoProducto { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Código alternativo del producto
        /// </summary>
        public string CodigoAlternativo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre alternativo del producto
        /// </summary>
        public string NombreAlternativo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción corta del producto
        /// </summary>
        public string DescripcionCorta { get; set; } = string.Empty;

        /// <summary>
        /// Descripción completa del producto
        /// </summary>
        public string? Descripcion { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // INFORMACIÓN PRINCIPAL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Tipo de producto (1 = Producto, 2 = Servicio, 3 = Paquete)
        /// </summary>
        public int TipoProducto { get; set; }

        /// <summary>
        /// Descripción del tipo de producto
        /// </summary>
        public string TipoProductoDescripcion => TipoProducto switch
        {
            1 => "Producto",
            2 => "Servicio",
            3 => "Paquete",
            _ => "Otro"
        };

        /// <summary>
        /// Fecha de alta del producto
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Status del producto (0 = Inactivo, 1 = Activo)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Indica si el producto está activo
        /// </summary>
        public bool EstaActivo => Status == 1;

        /// <summary>
        /// Control de existencia (0 = No, 1 = Sí)
        /// </summary>
        public int ControlExistencia { get; set; }

        /// <summary>
        /// Peso del producto
        /// </summary>
        public double Peso { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PRECIOS (10 niveles de precio)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Precio nivel 1
        /// </summary>
        public double Precio1 { get; set; }

        /// <summary>
        /// Precio nivel 2
        /// </summary>
        public double Precio2 { get; set; }

        /// <summary>
        /// Precio nivel 3
        /// </summary>
        public double Precio3 { get; set; }

        /// <summary>
        /// Precio nivel 4
        /// </summary>
        public double Precio4 { get; set; }

        /// <summary>
        /// Precio nivel 5
        /// </summary>
        public double Precio5 { get; set; }

        /// <summary>
        /// Precio nivel 6
        /// </summary>
        public double Precio6 { get; set; }

        /// <summary>
        /// Precio nivel 7
        /// </summary>
        public double Precio7 { get; set; }

        /// <summary>
        /// Precio nivel 8
        /// </summary>
        public double Precio8 { get; set; }

        /// <summary>
        /// Precio nivel 9
        /// </summary>
        public double Precio9 { get; set; }

        /// <summary>
        /// Precio nivel 10
        /// </summary>
        public double Precio10 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // COSTOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Costo estándar del producto
        /// </summary>
        public double CostoEstandar { get; set; }

        /// <summary>
        /// Método de costeo (1 = Promedio, 2 = PEPS, 3 = UEPS, 4 = Identificado)
        /// </summary>
        public int MetodoCosteo { get; set; }

        /// <summary>
        /// Margen de utilidad
        /// </summary>
        public double MargenUtilidad { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // IMPUESTOS Y RETENCIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Porcentaje de impuesto 1 (IVA)
        /// </summary>
        public double Impuesto1 { get; set; }

        /// <summary>
        /// Porcentaje de impuesto 2 (IEPS)
        /// </summary>
        public double Impuesto2 { get; set; }

        /// <summary>
        /// Porcentaje de impuesto 3
        /// </summary>
        public double Impuesto3 { get; set; }

        /// <summary>
        /// Porcentaje de retención 1
        /// </summary>
        public double Retencion1 { get; set; }

        /// <summary>
        /// Porcentaje de retención 2
        /// </summary>
        public double Retencion2 { get; set; }

        /// <summary>
        /// Indica si el producto es exento de impuestos
        /// </summary>
        public int EsExento { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // UNIDADES DE MEDIDA (FKs)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de unidad de medida base
        /// </summary>
        public int IdUnidadBase { get; set; }

        /// <summary>
        /// ID de unidad no convertible
        /// </summary>
        public int IdUnidadNoConvertible { get; set; }

        /// <summary>
        /// ID de unidad de compra
        /// </summary>
        public int IdUnidadCompra { get; set; }

        /// <summary>
        /// ID de unidad de venta
        /// </summary>
        public int IdUnidadVenta { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // MONEDA (FK)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de la moneda del producto
        /// </summary>
        public int IdMoneda { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CLASIFICACIONES (6 niveles)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de clasificación 1
        /// </summary>
        public int Clasificacion1 { get; set; }

        /// <summary>
        /// ID de clasificación 2
        /// </summary>
        public int Clasificacion2 { get; set; }

        /// <summary>
        /// ID de clasificación 3
        /// </summary>
        public int Clasificacion3 { get; set; }

        /// <summary>
        /// ID de clasificación 4
        /// </summary>
        public int Clasificacion4 { get; set; }

        /// <summary>
        /// ID de clasificación 5
        /// </summary>
        public int Clasificacion5 { get; set; }

        /// <summary>
        /// ID de clasificación 6
        /// </summary>
        public int Clasificacion6 { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CAMPOS ADICIONALES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Clave SAT para facturación electrónica
        /// </summary>
        public string ClaveSat { get; set; } = string.Empty;

        /// <summary>
        /// Permite existencia negativa (0 = No, 1 = Sí)
        /// </summary>
        public int ExistenciaNegativa { get; set; }

        /// <summary>
        /// Timestamp de última modificación
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════════
        // DIMENSIONES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Alto del producto
        /// </summary>
        public double Alto { get; set; }

        /// <summary>
        /// Largo del producto
        /// </summary>
        public double Largo { get; set; }

        /// <summary>
        /// Ancho del producto
        /// </summary>
        public double Ancho { get; set; }
    }
}