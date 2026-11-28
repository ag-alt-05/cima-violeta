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
        public DbSet<Ruta> Rutas { get; set; }
        public DbSet<UbicacionConductor> UbicacionesConductores { get; set; }
        public DbSet<VotoComentario> VotosComentarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comentario>().ToTable(tb => tb.UseSqlOutputClause(false));
            modelBuilder.Entity<VotoComentario>().ToTable(tb => tb.UseSqlOutputClause(false));
            modelBuilder.Entity<Usuario>().ToTable(tb => tb.UseSqlOutputClause(false));
        }
    }
}