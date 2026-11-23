using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CimaVioletaBackend.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }
        public string nombre_usuario { get; set; }
        public string nombre_acceso { get; set; }
        public string contrasena_hash { get; set; }
        public string? imagen_perfil { get; set; }
        public string? descripcion { get; set; }
        public bool es_admin { get; set; }
        public bool es_conductor { get; set; }
        public bool bloqueado { get; set; }
        public DateTime fecha_creacion { get; set; } = DateTime.Now;
    }
}