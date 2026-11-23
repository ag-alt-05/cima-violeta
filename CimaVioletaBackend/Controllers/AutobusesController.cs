using CimaVioletaBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CimaVioletaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutobusesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AutobusesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var autobuses = await _db.Autobuses
                .OrderBy(a => a.nombre_autobus)
                .Select(a => new {
                    a.id_autobus,
                    a.nombre_autobus,
                    a.id_ruta
                })
                .ToListAsync();

            return Ok(new { ok = true, autobuses });
        }
    }
}