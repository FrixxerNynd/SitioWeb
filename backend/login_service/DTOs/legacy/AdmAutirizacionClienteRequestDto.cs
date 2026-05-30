
namespace back_cabs.CRM.DTOs.Legacy
{
    public class AutorizarClienteRequestDto
    {
        // Datos financieros — todos opcionales; si vienen null se respeta el valor actual
        public int? ListaPrecio { get; set; }
        public double? LimiteCredito { get; set; }
        public int? DiasCredito { get; set; }
        public int? PermiteCredito { get; set; } // 1 = sí, 0 = no
        public int? PuedeExcederCredito { get; set; } // 1 = sí, 0 = no
        public double? DescuentoDocto { get; set; }
        public double? DescuentoMovto { get; set; }

        // Estatus y entrega
        public int? TipoEntrega { get; set; }
    }
}