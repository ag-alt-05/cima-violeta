using CimaVioletaBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CimaVioletaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParadasController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ParadasController(AppDbContext db) { _db = db; }

        // GET: api/paradas/1
        [HttpGet("{id_autobus}")]
        public async Task<IActionResult> ObtenerParadas(int id_autobus)
        {
            try
            {
                var autobus = await _db.Autobuses.FindAsync(id_autobus);
                if (autobus == null)
                    return NotFound(new { ok = false, error = "Autobús no encontrado" });

                // Buscamos las paradas que pertenecen a la ruta del autobús
                var paradas = await _db.Rutas
                    .Where(r => r.id_ruta == autobus.id_ruta)
                    .OrderBy(r => r.Orden)
                    .Select(r => new {
                        r.id_parada,
                        r.Nombre,
                        r.Latitud,
                        r.Longitud,
                        r.Orden,
                        r.EsWaypoint
                    })
                    .ToListAsync();

                return Ok(new { ok = true, paradas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }
        // GET: api/paradas/todas
        [HttpGet("todas")]
        public async Task<IActionResult> ObtenerTodasLasParadas()
        {
            var paradas = await _db.Rutas
                .Select(r => new {
                    r.id_parada,
                    r.Nombre,
                    r.Latitud,
                    r.Longitud,
                    r.id_ruta
                })
                .ToListAsync();

            return Ok(new { ok = true, paradas });
        }
    }
}