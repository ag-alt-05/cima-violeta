using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CimaVioletaBackend.Models
{
    [Table("VotosComentarios")]
    public class VotoComentario
    {
        [Key] public int id_voto { get; set; }
        public int id_comentario { get; set; }
        public int id_usuario { get; set; }
        public int valor { get; set; }
    }
}