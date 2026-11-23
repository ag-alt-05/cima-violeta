using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CimaVioletaBackend.Models
{
    [Table("Rutas")]
    public class Ruta
    {
        [Key]
        public int id_parada { get; set; }
        public int id_ruta { get; set; }
        public string Nombre { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public int Orden { get; set; }
        public bool EsWaypoint { get; set; }
    }
}