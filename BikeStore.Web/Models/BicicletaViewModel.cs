using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeStore.Web.Models
{
    public class BicicletaViewModel
    {
        public int IdBicicleta {  get; set; }
        public int IdCategoria { get; set; }        
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Estado { get; set; } = "Disponible";
        public string? CategoriaNombre { get; set; }
        public List<SelectListItem> CategoriasList { get; set; } = new();

    }
}
