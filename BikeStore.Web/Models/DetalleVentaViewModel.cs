namespace BikeStore.Web.Models
{
    public class DetalleVentaViewModel
    {
        public int IdDetalle { get; set; }
        public int IdVenta { get; set; }
        public int IdBicicleta { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }

    }
}
