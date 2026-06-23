using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;

namespace SistemaCitasMedicas.Controllers
{
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
            var cita = await _context.Citas
                .FirstOrDefaultAsync(x => x.IdCita == id);

            if (cita == null)
                return NotFound();

            var estadoCancelada = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.Nombre == "Cancelada");

            if (estadoCancelada == null)
                return BadRequest("No existe estado Cancelada");

            cita.IdEstadoCita = estadoCancelada.IdEstadoCita;
            cita.Observaciones = motivo;

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
    }
}