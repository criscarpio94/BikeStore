using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeStore.Web.Models
{
    public class VentaCreateViewModel
    {
        // Datos del Cliente
        public string? CedulaBusqueda { get; set; }
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }


        // Selección Temporal de Producto
        public int IdBicicletaSeleccionada { get; set; }
        public int CantidadSeleccionada { get; set; } = 1;


        // Lista de Productos Agregados a la Venta
        public List<DetalleItemViewModel> Detalles { get; set; } = new();


        // Totales Generales
        public decimal Subtotal => Detalles?.Sum(d => d.Subtotal) ?? 0m;
        public decimal Iva => Subtotal * 0.15m;
        public decimal Total => Subtotal + Iva;


        // Desplegables
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ClientesList { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> BicicletasList { get; set; } = new();
    }

    public class DetalleItemViewModel
    {
        public int IdBicicleta { get; set; }
        public string NombreBicicleta { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public int StockDisponible { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }
}
