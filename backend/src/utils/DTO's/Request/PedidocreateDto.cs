public class PedidoCreateDto
{
    public int UsuarioId { get; set; }
    public string Transportista { get; set; } = string.Empty;
    public List<int> ProductoIds { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Flete { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }
}