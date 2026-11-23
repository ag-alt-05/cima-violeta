using CimaVioletaBackend.Data;
using CimaVioletaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace CimaVioletaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ComentariosController> _logger;

        public ComentariosController(AppDbContext db, ILogger<ComentariosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("{id_autobus}")]
        public async Task<IActionResult> ObtenerComentarios(int id_autobus, int skip = 0, int id_usuario = 0)
        {
            bool esAdmin = false;
            if (id_usuario > 0)
            {
                var u = await _db.Usuarios.FindAsync(id_usuario);
                if (u != null && u.es_admin) esAdmin = true;
            }

            var query = _db.Comentarios.Where(c => c.id_autobus == id_autobus);

            // Si NO es admin, solo ve los aprobados. Si es admin, ve todo.
            if (!esAdmin) query = query.Where(c => c.es_aprobado == true);

            var comentarios = await query
                .OrderByDescending(c => c.votos)
                .ThenByDescending(c => c.fecha_creacion)
                .Skip(skip)
                .Take(10)
                .ToListAsync();

            var listaFinal = new List<object>();

            var promedio = 0.0;
            var total_aceptados = 0;

            foreach (var c in comentarios)
            {
                string nombreAutor = "Anónimo";
                if (c.id_usuario.HasValue)
                {
                    var user = await _db.Usuarios.FindAsync(c.id_usuario.Value);
                    if (user != null) nombreAutor = user.nombre_acceso;
                }

                int miVoto = 0;
                if (id_usuario > 0)
                {
                    var votoDb = await _db.VotosComentarios.FirstOrDefaultAsync(v => v.id_comentario == c.id_comentario && v.id_usuario == id_usuario);
                    if (votoDb != null) miVoto = votoDb.valor;
                }

                listaFinal.Add(new
                {
                    c.id_comentario,
                    c.contenido,
                    c.calificacion,
                    c.votos,
                    miVoto,
                    c.es_aprobado, // <-- Enviamos si está aprobado para pintarlo de rojo
                    fecha = c.fecha_creacion.ToString("dd/MM/yyyy"),
                    autor = nombreAutor,
                    es_anonimo = c.id_usuario == null
                });
                if(c.es_aprobado == true)
                {
                    promedio += c.calificacion;
                    total_aceptados++;
                }
            }
            var total = await query.CountAsync();
            promedio /= total_aceptados;

            return Ok(new { ok = true, comentarios = listaFinal, promedio = Math.Round(promedio, 1), total });
        }

        [HttpGet("usuario/{nombre_acceso}")]
        public async Task<IActionResult> ObtenerComentariosUsuario(string nombre_acceso)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.nombre_acceso == nombre_acceso);
            if (usuario == null) return NotFound(new { ok = false });

            var comentarios = await _db.Comentarios
                .Where(c => c.id_usuario == usuario.id_usuario)
                .Join(_db.Autobuses, c => c.id_autobus, a => a.id_autobus, (c, a) => new { c, a })
                .OrderByDescending(x => x.c.fecha_creacion)
                .Select(x => new {
                    x.c.id_comentario,
                    x.c.contenido,
                    x.c.calificacion,
                    x.c.es_aprobado,
                    x.c.votos, // <--- AGREGADO AQUÍ
                    fecha = x.c.fecha_creacion.ToString("dd/MM/yyyy"),
                    autobus = x.a.nombre_autobus
                }).ToListAsync();

            return Ok(new { ok = true, comentarios });
        }

        [HttpPost]
        public async Task<IActionResult> CrearComentario([FromBody] ComentarioDto req)
        {
            if (req.IdUsuario.HasValue)
            {
                var user = await _db.Usuarios.FindAsync(req.IdUsuario.Value);
                if (user != null && user.bloqueado) return BadRequest(new { ok = false, error = "Tu cuenta está bloqueada." });
            }

            if (string.IsNullOrWhiteSpace(req.Contenido) || req.Contenido.Trim().Length < 10)
                return BadRequest(new { ok = false, error = "Mínimo 10 caracteres." });

            if (req.Contenido.Length > 500)
                return BadRequest(new { ok = false, error = "Excede los 500 caracteres." });

            bool? estadoAprobacion = null;
            int? toxicidad = null;
            string mensaje = "Comentario enviado a revisión.";
            bool esAdmin = false;

            if (req.IdUsuario.HasValue)
            {
                var user = await _db.Usuarios.FindAsync(req.IdUsuario.Value);
                if (user != null && user.es_admin) { esAdmin = true; estadoAprobacion = true; mensaje = "Publicado (Admin)."; }
            }

            if (!esAdmin)
            {
                toxicidad = await ObtenerNivelToxicidad(req.Contenido);
                if (toxicidad == null || toxicidad >= 80) { estadoAprobacion = null; mensaje = "Enviado a revisión."; }
                else { estadoAprobacion = true; mensaje = "Publicado exitosamente."; }
            }

            var nuevo = new Comentario
            {
                id_autobus = req.IdAutobus,
                id_usuario = req.IdUsuario,
                contenido = req.Contenido.Trim(),
                calificacion = req.Calificacion,
                es_aprobado = estadoAprobacion,
                nivel_toxicidad = toxicidad,
                votos = 0 // INICIA EN 0
            };

            _db.Comentarios.Add(nuevo);
            await _db.SaveChangesAsync();

            return Ok(new { ok = true, mensaje = mensaje });
        }

        [HttpPost("votar")]
        public async Task<IActionResult> Votar([FromBody] VotoDto req)
        {
            if (req.IdUsuario == 0 || req.IdComentario == 0) return BadRequest();
            var comentario = await _db.Comentarios.FindAsync(req.IdComentario);
            if (comentario == null) return NotFound();

            var votoExistente = await _db.VotosComentarios.FirstOrDefaultAsync(v => v.id_comentario == req.IdComentario && v.id_usuario == req.IdUsuario);
            int nuevoVotoUsuario = 0; // 0 significa que quitó su voto

            if (votoExistente != null)
            {
                if (votoExistente.valor == req.Valor) // Si da clic en el mismo, quita el voto
                {
                    _db.VotosComentarios.Remove(votoExistente);
                    comentario.votos -= req.Valor;
                }
                else // Si cambia de upvote a downvote o viceversa
                {
                    comentario.votos -= votoExistente.valor; // Quita el voto anterior
                    comentario.votos += req.Valor;           // Suma el nuevo
                    votoExistente.valor = req.Valor;
                    nuevoVotoUsuario = req.Valor;
                }
            }
            else
            {
                _db.VotosComentarios.Add(new VotoComentario { id_comentario = req.IdComentario, id_usuario = req.IdUsuario, valor = req.Valor });
                comentario.votos += req.Valor;
                nuevoVotoUsuario = req.Valor;
            }

            await _db.SaveChangesAsync();
            return Ok(new { ok = true, nuevosVotos = comentario.votos, miVoto = nuevoVotoUsuario });
        }

        public class VotoDto { public int IdComentario { get; set; } public int IdUsuario { get; set; } public int Valor { get; set; } }

        [HttpDelete("{id_comentario}/{nombre_acceso}")]
        public async Task<IActionResult> EliminarComentario(int id_comentario, string nombre_acceso)
        {
            var admin = await _db.Usuarios.FirstOrDefaultAsync(u => u.nombre_acceso == nombre_acceso);
            if (admin == null || !admin.es_admin) return Unauthorized(new { ok = false, error = "Sin permisos." });

            var comentario = await _db.Comentarios.FindAsync(id_comentario);
            if (comentario == null) return NotFound();

            _db.Comentarios.Remove(comentario);
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }

        [HttpPut("estado/{id_comentario}")]
        public async Task<IActionResult> CambiarEstado(int id_comentario, [FromBody] EstadoDto req)
        {
            var admin = await _db.Usuarios.FirstOrDefaultAsync(u => u.nombre_acceso == req.NombreAdmin);
            if (admin == null || !admin.es_admin) return Unauthorized(new { ok = false, error = "Sin permisos." });

            var comentario = await _db.Comentarios.FindAsync(id_comentario);
            if (comentario == null) return NotFound();

            comentario.es_aprobado = req.Aprobado;
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }

        public class EstadoDto { public bool Aprobado { get; set; } public string NombreAdmin { get; set; } }
        private async Task<int?> ObtenerNivelToxicidad(string texto)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var url = "http://127.0.0.1:8000/clasificar";
                var content = new StringContent(JsonSerializer.Serialize(new { texto = texto }), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode) return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("toxicidad_porcentaje", out var toxProp)) return toxProp.GetInt32();
                return null;
            }
            catch { return null; }
        }
    }
    public class ComentarioDto { public int IdAutobus { get; set; } public int? IdUsuario { get; set; } public string Contenido { get; set; } = string.Empty; public int Calificacion { get; set; } }
}