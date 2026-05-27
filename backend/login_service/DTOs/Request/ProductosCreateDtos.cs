public class ProductosCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal PrecioOriginal { get; set; }
    public int Stock { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CodigoSAT { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Subcategoria { get; set; } = string.Empty;
}