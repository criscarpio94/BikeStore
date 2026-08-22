using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BikeStore.Api.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }

        [JsonIgnore]
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
