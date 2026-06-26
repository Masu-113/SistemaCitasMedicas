using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize]
    public class SolicitudesMedicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesMedicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LISTADO SOLICITUDES
        // =========================
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesCita
                .Include(x => x.Paciente)
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

        // =========================
        // ACEPTAR SOLICITUD
        // =========================
        [HttpPost]
        public async Task<IActionResult> Aceptar(int id, int idMedico)
        {
            var solicitud = await _context.SolicitudesCita
                .FirstOrDefaultAsync(x => x.IdSolicitud == id);

            if (solicitud == null)
                return NotFound();

            var medico = await _context.Medicos
                .FirstOrDefaultAsync(x => x.IdMedico == idMedico);

            if (medico == null)
                return BadRequest("Médico no encontrado");

            // =========================
            // ESTADOS
            // =========================
            var estadoAceptada = await _context.EstadosSolicitud
                .FirstAsync(x => x.Nombre == "Aceptada");

            var estadoCita = await _context.EstadosCita
                .FirstAsync(x => x.Nombre == "Pendiente");

            solicitud.IdEstadoSolicitud = estadoAceptada.IdEstadoSolicitud;

            // =========================
            // NORMALIZAR DÍA SEMANA
            // =========================
            var diaSemana = (byte)solicitud.FechaDeseada.DayOfWeek;
            if (diaSemana == 0) diaSemana = 7;

            // =========================
            // VALIDAR HORARIO MÉDICO
            // =========================
            var dentroHorario = await _context.HorariosMedico.AnyAsync(h =>
                h.IdMedico == idMedico &&
                h.DiaSemana == diaSemana &&
                solicitud.HoraDeseada >= h.HoraInicio &&
                solicitud.HoraDeseada < h.HoraFin
            );

            if (!dentroHorario)
                return BadRequest("El médico no está disponible en ese horario.");

            // =========================
            // VALIDAR CONFLICTO CITAS
            // =========================
            var horaInicio = solicitud.HoraDeseada;
            var horaFin = solicitud.HoraDeseada.AddMinutes(medico.DuracionCitaMin);

            var conflicto = await _context.Citas.AnyAsync(c =>
                c.IdMedico == idMedico &&
                c.FechaCita == solicitud.FechaDeseada &&
                c.IdEstadoCita != estadoCita.IdEstadoCita && // opcional refuerzo
                c.IdEstadoCita != _context.EstadosCita.First(e => e.Nombre == "Cancelada").IdEstadoCita &&
                c.IdEstadoCita != _context.EstadosCita.First(e => e.Nombre == "Finalizada").IdEstadoCita &&
                (
                    horaInicio < c.HoraFin &&
                    horaFin > c.HoraInicio
                )
            );

            if (conflicto)
                return BadRequest("El médico ya tiene una cita en ese horario.");

            // =========================
            // CREAR CITA
            // =========================
            var cita = new Cita
            {
                IdSolicitud = solicitud.IdSolicitud,
                IdMedico = medico.IdMedico,
                FechaCita = solicitud.FechaDeseada,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                IdEstadoCita = estadoCita.IdEstadoCita,
                Observaciones = solicitud.Motivo
            };

            _context.Citas.Add(cita);

            // =========================
            // HISTORIAL
            // =========================
            _context.HistorialesSolicitud.Add(new HistorialSolicitud
            {
                IdSolicitud = solicitud.IdSolicitud,
                IdMedico = medico.IdMedico,
                Accion = "ACEPTADA",
                Comentario = "Solicitud aceptada y cita creada",
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CANCELAR CITA
        // =========================
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return BadRequest("Debe ingresar un motivo de cancelación");

            var cita = await _context.Citas
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(x => x.IdCita == id);

            if (cita == null)
                return NotFound();

            var estadoCancelada = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.Nombre == "Cancelada");

            if (estadoCancelada == null)
                return BadRequest("No existe el estado Cancelada");

            cita.IdEstadoCita = estadoCancelada.IdEstadoCita;
            cita.Observaciones = motivo;

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

        // =========================
        // MÉDICOS DISPONIBLES
        // =========================
        private async Task<List<Medico>> GetMedicosDisponibles(SolicitudCita solicitud)
        {
            var diaSemana = (byte)solicitud.FechaDeseada.DayOfWeek;
            if (diaSemana == 0) diaSemana = 7;

            var horaInicio = solicitud.HoraDeseada;
            var horaFin = solicitud.HoraDeseada.AddMinutes(30);

            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Where(m => m.Activo)
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