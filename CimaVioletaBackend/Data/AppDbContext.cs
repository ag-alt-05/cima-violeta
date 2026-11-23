using CimaVioletaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CimaVioletaBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Autobus> Autobuses { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<UbicacionConductor> UbicacionesConductores { get; set; }

        public DbSet<Ruta> Rutas { get; set; }
        public DbSet<VotoComentario> VotosComentarios { get; set; }
    }
}