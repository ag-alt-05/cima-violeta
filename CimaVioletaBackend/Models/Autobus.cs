using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CimaVioletaBackend.Models
{
    [Table("Autobuses")]
    public class Autobus
    {
        [Key]
        public int id_autobus { get; set; }
        public string nombre_autobus { get; set; }
        public int id_ruta { get; set; }
    }
}