using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CimaVioletaBackend.Models
{
    [Table("Comentarios")]
    public class Comentario
    {
        [Key]
        public int id_comentario { get; set; }
        public int? id_usuario { get; set; }
        public int id_autobus { get; set; }
        public string contenido { get; set; }
        public int calificacion { get; set; }
        public bool? es_aprobado { get; set; }
        public int? nivel_toxicidad { get; set; }
        public long votos { get; set; }
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
    }
}