using System.Text.Json.Serialization;

namespace BikeStore.Api.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        [JsonIgnore]
        public ICollection<Bicicleta> ? Bicicletas { get; set; }
    }
}

