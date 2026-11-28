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
                // Insertar nuevo registro usando SQL puro para evitar triggers
                await _db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO UbicacionesConductores (id_conductor, estado_compartir, ultima_actualizacion) VALUES ({0}, 1, GETDATE())",
                    id_conductor);
                return Ok(new { ok = true, estado = true });
            }
            else
            {
                // Actualizar registro existente
                bool nuevoEstado = !ubicacion.estado_compartir;
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE UbicacionesConductores SET estado_compartir = {0}, ultima_actualizacion = GETDATE() WHERE id_conductor = {1}",
                    nuevoEstado, id_conductor);
                return Ok(new { ok = true, estado = nuevoEstado });
            }
        }

        // 2. EL CONDUCTOR ENVÍA SUS COORDENADAS (Usando el Stored Procedure)
        [HttpPost("actualizar")]
        public async Task<IActionResult> ActualizarUbicacion([FromBody] UbicacionDto req)
        {
            var ubicacion = await _db.UbicacionesConductores.FindAsync(req.IdConductor);

            if (ubicacion != null && ubicacion.estado_compartir)
            {
                // Ejecutando tu SP que ya tienes en la base de datos
                await _db.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_ActualizarUbicacionConductor @id_conductor = {0}, @id_autobus = {1}, @latitud = {2}, @longitud = {3}, @compartir = 1",
                    req.IdConductor, req.IdAutobus ?? 0, req.Latitud ?? 0.0, req.Longitud ?? 0.0
                );
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
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE UbicacionesConductores SET estado_compartir = 0, ultima_actualizacion = GETDATE() WHERE id_conductor = {0}",
                id_conductor);
            return Ok(new { ok = true });
        }
    }

    public class UbicacionDto
    {
        public int IdConductor { get; set; }
        public double? Latitud { get; set; }  // <-- Cambiado a double por el FLOAT de SQL
        public double? Longitud { get; set; } // <-- Cambiado a double por el FLOAT de SQL
        public int? IdAutobus { get; set; }
    }
}