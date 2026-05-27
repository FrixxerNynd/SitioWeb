public class PedidoResponseDto
{
    public int Id { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Transportista { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public List<string> Productos { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Flete { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }
}