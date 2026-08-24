namespace BikeStore.Web.Models
{
    public class VentaViewModel
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public string CedulaCliente { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Iva {  get; set; }
        public decimal Total { get; set; }
        public List<DetalleItemViewModel> Detalles { get; set; } = new();

    }
}
