using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CimaVioletaBackend.Models
{
    [Table("UbicacionesConductores")]
    public class UbicacionConductor
    {
        [Key]
        public int id_conductor { get; set; }

        public double? latitud { get; set; }
        public double? longitud { get; set; }
        public DateTime ultima_actualizacion { get; set; } = DateTime.Now;

        public int? id_autobus { get; set; }
        public bool estado_compartir { get; set; }
    }
}