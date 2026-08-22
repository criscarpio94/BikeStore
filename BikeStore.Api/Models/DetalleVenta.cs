using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BikeStore.Api.Models
{
    public class DetalleVenta
    {
        public int IdDetalle { get; set; }
        public int IdVenta { get; set; }
        public int IdBicicleta { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }

        //PARA INDICAR A FRAMEWORK QUE SQL CALCULA ESTA COLUMNA
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Subtotal { get; set; }
        [JsonIgnore]
        public Venta? Venta { get; set; }
        public Bicicleta? Bicicleta { get; set; }
    }
}

