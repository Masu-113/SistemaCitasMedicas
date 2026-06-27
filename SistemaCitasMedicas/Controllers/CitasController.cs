using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using System.Security.Claims;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize(Roles = "Administrador,Paciente,Medico")]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO DE CITAS
        // LISTADO DE CITAS
        public async Task<IActionResult> Index()
        {
            var query = _context.Citas
                .Include(c => c.EstadoCita)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Paciente)
                        .ThenInclude(p => p.Usuario)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Especialidad)
                .AsQueryable();


            if (User.IsInRole("Medico"))
            {
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                var medico = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Usuario.IdUsuario == int.Parse(userId));

                if (medico == null)
                    return BadRequest("Usuario médico no encontrado");


                query = query.Where(c => c.IdMedico == medico.IdMedico);
            }


            if (User.IsInRole("Paciente"))
            {
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                var paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Usuario.IdUsuario == int.Parse(userId));

                if (paciente == null)
                    return BadRequest("Usuario paciente no encontrado");


                query = query.Where(c =>
                    c.Solicitud.IdPaciente == paciente.IdPaciente
                );
            }


            var citas = await query
                .OrderByDescending(c => c.FechaCita)
                .ToListAsync();


            ViewBag.EstadosCita = await _context.EstadosCita
                .ToListAsync();


            return View(citas);
        }


        // DETALLE DE CITA
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cita = await _context.Citas
                .Include(c => c.EstadoCita)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Paciente)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Especialidad)
                .FirstOrDefaultAsync(m => m.IdCita == id);

            if (cita == null)
                return NotFound();

            return View(cita);
        }

        // CANCELAR CITA (EN VEZ DE DELETE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return BadRequest("Debe ingresar un motivo de cancelación");

            var cita = await _context.Citas
                .FirstOrDefaultAsync(x => x.IdCita == id);

            if (cita == null)
                return NotFound();

            var estadoCancelada = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.Nombre == "Cancelada");

            if (estadoCancelada == null)
                return BadRequest("No existe estado Cancelada");

            // ✔ SOLO CAMBIAR ESTADO
            cita.IdEstadoCita = estadoCancelada.IdEstadoCita;

            // ✔ SOLO HISTORIAL (FUENTE DE VERDAD)
            _context.HistorialesSolicitud.Add(new HistorialSolicitud
            {
                IdSolicitud = cita.IdSolicitud,
                IdMedico = cita.IdMedico,
                Accion = "CITA_CANCELADA",
                Comentario = motivo,
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // EDIT LIMITADO (OPCIONAL - solo para ajustes de fecha/hora si quieres)
        [HttpPost]
        public async Task<IActionResult> Reprogramar(int id, DateOnly fecha, TimeOnly horaInicio, int duracionMin)
        {
            var cita = await _context.Citas
                .FirstOrDefaultAsync(x => x.IdCita == id);

            if (cita == null)
                return NotFound();

            cita.FechaCita = fecha;
            cita.HoraInicio = horaInicio;
            cita.HoraFin = horaInicio.AddMinutes(duracionMin);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Cambiar el estado de la cita.
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int idCita, int idEstadoCita)
        {
            var cita = await _context.Citas
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(x => x.IdCita == idCita);

            if (cita == null)
                return NotFound();

            var estadoDb = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.IdEstadoCita == idEstadoCita);

            if (estadoDb == null)
                return BadRequest("Estado inválido");

            // ✔ actualizar estado
            cita.IdEstadoCita = estadoDb.IdEstadoCita;

            // ✔ historial (sin romper datos de Observaciones)
            _context.HistorialesSolicitud.Add(new HistorialSolicitud
            {
                IdSolicitud = cita.IdSolicitud,
                IdMedico = cita.IdMedico,
                Accion = $"CITA_{estadoDb.Nombre.ToUpper()}",
                Comentario = $"Cambio de estado a {estadoDb.Nombre}",
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}