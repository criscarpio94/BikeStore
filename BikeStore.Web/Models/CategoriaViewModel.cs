namespace BikeStore.Web.Models
{
    public class CategoriaViewModel
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion {  get; set; }
        public bool Activo { get; set; } = true;
    }
}
