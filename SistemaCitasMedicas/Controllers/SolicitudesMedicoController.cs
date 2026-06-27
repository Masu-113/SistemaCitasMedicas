using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using System.Security.Claims;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize(Roles = "Administrador,Medico")]
    public class SolicitudesMedicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesMedicoController(ApplicationDbContext context)
        {
            _context = context;
        }
        // LISTADO SOLICITUDES
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesCita
                .Include(x => x.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(x => x.Especialidad)
                .Where(x => x.IdEstadoSolicitud == 1)
                .ToListAsync();

            var medicosDisponibles = new Dictionary<int, List<Medico>>();

            foreach (var sol in solicitudes)
            {
                medicosDisponibles[sol.IdSolicitud] = await GetMedicosDisponibles(sol);
            }

            ViewBag.MedicosDisponibles = medicosDisponibles;

            return View(solicitudes);
        }

        // ACEPTAR SOLICITUD
        [HttpPost]
        public async Task<IActionResult> Aceptar(int id, int? idMedico)
        {
            var solicitud = await _context.SolicitudesCita
                .FirstOrDefaultAsync(x => x.IdSolicitud == id);

            if (solicitud == null)
                return NotFound();

            var estadoAceptada = await _context.EstadosSolicitud
                .FirstAsync(x => x.Nombre == "Aceptada");

            var estadoCita = await _context.EstadosCita
                .FirstAsync(x => x.Nombre == "Pendiente");

            int medicoFinalId;

            if (User.IsInRole("Medico"))
            {
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                var medicoLogueado = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Usuario.IdUsuario == int.Parse(userId));

                if (medicoLogueado == null)
                    return BadRequest("No es un médico válido");

                medicoFinalId = medicoLogueado.IdMedico;
            }
            else
            {
                if (idMedico == null)
                    return BadRequest("Debe seleccionar un médico");

                medicoFinalId = idMedico.Value;
            }

            solicitud.IdEstadoSolicitud = estadoAceptada.IdEstadoSolicitud;

            var medico = await _context.Medicos.FindAsync(medicoFinalId);

            var diaSemana = (byte)solicitud.FechaDeseada.DayOfWeek;
            if (diaSemana == 0) diaSemana = 7;

            var dentroHorario = await _context.HorariosMedico.AnyAsync(h =>
                h.IdMedico == medicoFinalId &&
                h.DiaSemana == diaSemana &&
                solicitud.HoraDeseada >= h.HoraInicio &&
                solicitud.HoraDeseada < h.HoraFin
            );

            if (!dentroHorario)
                return BadRequest("Fuera de horario del médico");

            var horaInicio = solicitud.HoraDeseada;
            var horaFin = solicitud.HoraDeseada.AddMinutes(medico.DuracionCitaMin);

            var conflicto = await _context.Citas.AnyAsync(c =>
                c.IdMedico == medicoFinalId &&
                c.FechaCita == solicitud.FechaDeseada &&
                horaInicio < c.HoraFin &&
                horaFin > c.HoraInicio
            );

            if (conflicto)
                return BadRequest("Conflicto de horario");

            var cita = new Cita
            {
                IdSolicitud = solicitud.IdSolicitud,
                IdMedico = medicoFinalId,
                FechaCita = solicitud.FechaDeseada,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                IdEstadoCita = estadoCita.IdEstadoCita,
                Observaciones = solicitud.Motivo
            };

            _context.Citas.Add(cita);

            _context.HistorialesSolicitud.Add(new HistorialSolicitud
            {
                IdSolicitud = solicitud.IdSolicitud,
                IdMedico = medicoFinalId,
                Accion = "ACEPTADA",
                Comentario = "Solicitud aceptada",
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // CANCELAR CITA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return BadRequest("Debe ingresar un motivo");

            var solicitud = await _context.SolicitudesCita
                .FirstOrDefaultAsync(x => x.IdSolicitud == id);

            if (solicitud == null)
                return NotFound();

            // Estado "Cancelada"
            var estadoCancelada = await _context.EstadosSolicitud
                .FirstOrDefaultAsync(x => x.Nombre == "Cancelada");

            if (estadoCancelada == null)
                return BadRequest("No existe el estado Cancelada");

            // Cambiar estado de la solicitud
            solicitud.IdEstadoSolicitud = estadoCancelada.IdEstadoSolicitud;
            solicitud.ComentarioRespuesta = motivo;

            // Obtener médico logueado (si aplica)
            int? medicoId = null;

            if (User.IsInRole("Medico"))
            {
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                var medico = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Usuario.IdUsuario == int.Parse(userId));

                if (medico != null)
                {
                    medicoId = medico.IdMedico;
                }
            }

            // Guardar historial
            _context.HistorialesSolicitud.Add(new HistorialSolicitud
            {
                IdSolicitud = solicitud.IdSolicitud,
                IdMedico = medicoId,
                Accion = "CANCELADA",
                Comentario = motivo,
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // MÉDICOS DISPONIBLES
        private async Task<List<Medico>> GetMedicosDisponibles(SolicitudCita solicitud)
        {
            var diaSemana = (byte)solicitud.FechaDeseada.DayOfWeek;
            if (diaSemana == 0) diaSemana = 7;

            var horaInicio = solicitud.HoraDeseada;
            var horaFin = solicitud.HoraDeseada.AddMinutes(30);

            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Where(m => m.Usuario.Activo)
                .Where(m =>
                    _context.HorariosMedico.Any(h =>
                        h.IdMedico == m.IdMedico &&
                        h.DiaSemana == diaSemana &&
                        horaInicio >= h.HoraInicio &&
                        horaInicio < h.HoraFin
                    )
                )
                .Where(m =>
                    !_context.Citas.Any(c =>
                        c.IdMedico == m.IdMedico &&
                        c.FechaCita == solicitud.FechaDeseada &&
                        c.IdEstadoCita != _context.EstadosCita.First(e => e.Nombre == "Cancelada").IdEstadoCita &&
                        c.IdEstadoCita != _context.EstadosCita.First(e => e.Nombre == "Finalizada").IdEstadoCita &&
                        horaInicio < c.HoraFin &&
                        horaFin > c.HoraInicio
                    )
                )
                .ToListAsync();

            return medicos;
        }
    }
}