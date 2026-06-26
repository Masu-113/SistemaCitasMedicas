using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO DE CITAS
        public async Task<IActionResult> Index()
        {
            var citas = await _context.Citas
                .Include(c => c.EstadoCita)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Paciente)
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s.Especialidad)
                .OrderByDescending(c => c.FechaCreacion)
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
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return BadRequest("Debe ingresar un motivo de cancelación");
            }


            var cita = await _context.Citas
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(x => x.IdCita == id);


            if (cita == null)
                return NotFound();


            var estadoCancelada = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.Nombre == "Cancelada");


            if (estadoCancelada == null)
                return BadRequest("No existe estado Cancelada");


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
        public async Task<IActionResult> CambiarEstado(int idCita, string estado)
        {
            var cita = await _context.Citas
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(x => x.IdCita == idCita);

            if (cita == null)
                return NotFound();

            var estadoDb = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.Nombre == estado);

            if (estadoDb == null)
                return BadRequest("Estado inválido");

            // ✔ CAMBIO DE ESTADO DE LA CITA
            cita.IdEstadoCita = estadoDb.IdEstadoCita;

            // ✔ REGISTRO EN HISTORIAL SOLICITUD (AQUÍ ESTÁ LA CLAVE)
            _context.HistorialesSolicitud.Add(new HistorialSolicitud
            {
                IdSolicitud = cita.IdSolicitud,
                IdMedico = cita.IdMedico,
                Accion = $"CITA_{estado.ToUpper()}",
                Comentario = $"Cambio de estado de cita a {estado}",
                Fecha = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}