using CimaVioletaBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CimaVioletaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EstadoController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("verificar")]
        public async Task<IActionResult> VerificarConexion()
        {
            try
            {
                await _db.Database.OpenConnectionAsync();
                await _db.Database.CloseConnectionAsync();

                return Ok(new
                {
                    encendido = true,
                    mensaje = "Servidor encendido y conectado a la base de datos"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    encendido = false,
                    mensaje = "Servidor encendido, pero NO se pudo conectar a la base de datos",
                    error = ex.Message
                });
            }
        }
    }
}