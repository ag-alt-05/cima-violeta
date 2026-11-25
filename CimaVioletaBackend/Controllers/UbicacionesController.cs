using CimaVioletaBackend.Data;
using CimaVioletaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CimaVioletaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UbicacionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UbicacionesController(AppDbContext db) { _db = db; }

        // 1. EL CONDUCTOR PRENDE/APAGA EL SWITCH
        [HttpPost("toggle/{id_conductor}")]
        public async Task<IActionResult> ToggleCompartir(int id_conductor)
        {
            var ubicacion = await _db.UbicacionesConductores.FindAsync(id_conductor);

            if (ubicacion == null)
            {
                ubicacion = new UbicacionConductor
                {
                    id_conductor = id_conductor,
                    estado_compartir = true,
                    ultima_actualizacion = DateTime.Now
                };
                _db.UbicacionesConductores.Add(ubicacion);
            }
            else
            {
                ubicacion.estado_compartir = !ubicacion.estado_compartir;
                ubicacion.ultima_actualizacion = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return Ok(new { ok = true, estado = ubicacion.estado_compartir });
        }

        // 2. EL CONDUCTOR ENVÍA SUS COORDENADAS CADA 30 SEGUNDOS
        [HttpPost("actualizar")]
        public async Task<IActionResult> ActualizarUbicacion([FromBody] UbicacionDto req)
        {
            var ubicacion = await _db.UbicacionesConductores.FindAsync(req.IdConductor);

            if (ubicacion != null && ubicacion.estado_compartir)
            {
                ubicacion.latitud = req.Latitud;
                ubicacion.longitud = req.Longitud;
                ubicacion.id_autobus = req.IdAutobus;
                ubicacion.ultima_actualizacion = DateTime.Now;
                await _db.SaveChangesAsync();
                return Ok(new { ok = true, mensaje = "Ubicación actualizada" });
            }

            return BadRequest(new { ok = false, error = "El switch está apagado o no existe el registro." });
        }

        // 3. LOS USUARIOS NORMALES PIDEN LAS UBICACIONES ACTIVAS
        [HttpGet("activas")]
        public async Task<IActionResult> ObtenerUbicaciones()
        {
            var haceDosMinutos = DateTime.Now.AddMinutes(-2);

            var ubicaciones = await _db.UbicacionesConductores
                .Where(u => u.estado_compartir == true
                         && u.latitud != null
                         && u.longitud != null
                         && u.ultima_actualizacion >= haceDosMinutos)
                .Join(_db.Usuarios, ubi => ubi.id_conductor, usu => usu.id_usuario, (ubi, usu) => new { ubi, usu })
                .Join(_db.Autobuses, combo => combo.ubi.id_autobus, bus => bus.id_autobus, (combo, bus) => new { combo.ubi, combo.usu, bus })
                .Select(x => new {
                    id_conductor = x.ubi.id_conductor,
                    latitud = x.ubi.latitud,
                    longitud = x.ubi.longitud,
                    ultima_actualizacion = x.ubi.ultima_actualizacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                    nombre_conductor = x.usu.nombre_usuario,
                    nombre_autobus = x.bus.nombre_autobus
                })
                .ToListAsync();

            return Ok(new { ok = true, ubicaciones });
        }

        // 4. EL ADMIN VE LA LISTA DE CONDUCTORES
        [HttpGet("conductores")]
        public async Task<IActionResult> ListarConductores()
        {
            var conductores = await _db.Usuarios
                .Where(u => u.es_conductor == true)
                .Select(u => new { u.id_usuario, u.nombre_usuario, u.nombre_acceso })
                .ToListAsync();

            var ubicaciones = await _db.UbicacionesConductores.ToListAsync();

            var resultado = conductores.Select(c => new {
                c.id_usuario,
                c.nombre_usuario,
                c.nombre_acceso,
                transmitiendo = ubicaciones.FirstOrDefault(u => u.id_conductor == c.id_usuario)?.estado_compartir ?? false
            });

            return Ok(new { ok = true, conductores = resultado });
        }

        // 5. EL ADMIN APAGA FORZOSAMENTE A UN CONDUCTOR
        [HttpPost("apagar/{id_conductor}")]
        public async Task<IActionResult> ApagarUbicacion(int id_conductor)
        {
            var ubicacion = await _db.UbicacionesConductores.FindAsync(id_conductor);
            if (ubicacion != null)
            {
                ubicacion.estado_compartir = false;
                ubicacion.ultima_actualizacion = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            return Ok(new { ok = true });
        }
    }

    public class UbicacionDto
    {
        public int IdConductor { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public int? IdAutobus { get; set; }
    }
}