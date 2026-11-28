using CimaVioletaBackend.Data;
using CimaVioletaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CimaVioletaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsuariosController(AppDbContext db) { _db = db; }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Acceso) || string.IsNullOrWhiteSpace(req.Pass))
                return BadRequest(new { ok = false, error = "Todos los campos son obligatorios." });

            if (!Regex.IsMatch(req.Acceso, @"^[a-zA-Z0-9_-]+$")){
                return BadRequest(new { ok = false, error = "El usuario solo puede contener letras, números, guiones (-) y guiones bajos (_). No se permiten espacios ni otros símbolos..." });
            }

            if (req.Acceso.Length > 30 || req.Nombre.Length > 100)
{
                return BadRequest(new { ok = false, error = "El nombre de usuario o el nombre visible es demasiado largo." });
}
            var existe = await _db.Usuarios.AnyAsync(u => u.nombre_acceso == req.Acceso);
            if (existe) return BadRequest(new { ok = false, error = "El nombre de usuario ya está en uso." });

            var nuevo = new Usuario
            {
                nombre_usuario = req.Nombre,
                nombre_acceso = req.Acceso,
                contrasena_hash = req.Pass,
                imagen_perfil = req.Foto,
                es_admin = false,
                es_conductor = false,
                bloqueado = false
            };

            _db.Usuarios.Add(nuevo);
            await _db.SaveChangesAsync();

            return Ok(new { ok = true, mensaje = "Usuario creado exitosamente." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto req)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.nombre_acceso == req.Acceso && u.contrasena_hash == req.Pass);

            if (user == null) return Unauthorized(new { ok = false, error = "Usuario o contraseña incorrectos" });

            return Ok(new
            {
                ok = true,
                usuario = new
                {
                    user.id_usuario,
                    user.nombre_usuario,
                    user.nombre_acceso,
                    user.imagen_perfil,
                    user.descripcion,
                    user.es_admin,
                    user.es_conductor,
                    user.bloqueado,
                    fecha = user.fecha_creacion.ToString("yyyy-MM-dd")
                }
            });
        }

        [HttpGet("verificar")]
        public async Task<IActionResult> VerificarConexion()
        {
            try
            {
                await _db.Database.OpenConnectionAsync();
                await _db.Database.CloseConnectionAsync();
                return Ok(new { conectado = true, mensaje = "Conectado a la base de datos RutaVioleta" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { conectado = false, mensaje = "Error de conexión", error = ex.Message });
            }
        }

        [HttpGet("perfil/{nombre_acceso}")]
        public async Task<IActionResult> ObtenerPerfil(string nombre_acceso)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.nombre_acceso == nombre_acceso);
            if (user == null) return NotFound(new { ok = false, error = "Usuario no encontrado" });

            return Ok(new
            {
                ok = true,
                usuario = new
                {
                    user.id_usuario,
                    user.nombre_usuario,
                    user.nombre_acceso,
                    user.imagen_perfil,
                    user.descripcion,
                    user.es_admin,
                    user.es_conductor,
                    user.bloqueado,
                    fecha = user.fecha_creacion.ToString("yyyy-MM-dd")
                }
            });
        }

        [HttpPut("actualizar/{nombre_acceso}")]
        public async Task<IActionResult> ActualizarPerfil(string nombre_acceso, [FromBody] ActualizarPerfilDto req)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.nombre_acceso == nombre_acceso);
            if (user == null) return NotFound(new { ok = false, error = "Usuario no encontrado" });

            user.descripcion = req.Descripcion;
            if (!string.IsNullOrWhiteSpace(req.Foto)) user.imagen_perfil = req.Foto;

            await _db.SaveChangesAsync();
            return Ok(new { ok = true, mensaje = "Perfil actualizado correctamente" });
        }
    }

    public class RegistroDto { public string Nombre { get; set; } public string Acceso { get; set; } public string Pass { get; set; } public string? Foto { get; set; } }
    public class LoginDto { public string Acceso { get; set; } public string Pass { get; set; } }
    public class ActualizarPerfilDto { public string? Descripcion { get; set; } public string? Foto { get; set; } }
}