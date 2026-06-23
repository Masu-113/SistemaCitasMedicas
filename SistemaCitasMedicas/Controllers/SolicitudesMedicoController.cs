using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;

namespace SistemaCitasMedicas.Controllers
{
    public class SolicitudesMedicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesMedicoController(ApplicationDbContext context)
        {
            _context = context;
        }


        // Ver solicitudes pendientes
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesCita
                .Include(x => x.Paciente)
                .Include(x => x.Especialidad)
                .Where(x => x.IdEstadoSolicitud == 1)
                .ToListAsync();


            ViewBag.Medicos = await _context.Medicos
                .Include(x => x.Usuario)
                .Where(x => x.Activo)
                .ToListAsync();


            return View(solicitudes);
        }



        // Aceptar solicitud
        [HttpPost]
        public async Task<IActionResult> Aceptar(int id, int idMedico)
        {
            var solicitud = await _context.SolicitudesCita
                .FirstOrDefaultAsync(x => x.IdSolicitud == id);


            if (solicitud == null)
            {
                return NotFound();
            }


            var estadoAceptada = await _context.EstadosSolicitud
                .FirstAsync(x => x.Nombre == "Aceptada");


            solicitud.IdEstadoSolicitud = estadoAceptada.IdEstadoSolicitud;



            var estadoCita = await _context.EstadosCita
                .FirstAsync(x => x.Nombre == "Pendiente");



            var medico = await _context.Medicos
                .FirstOrDefaultAsync(x => x.IdMedico == idMedico);


            if (medico == null)
            {
                return BadRequest("Médico no encontrado");
            }



            var cita = new Cita
            {
                IdSolicitud = solicitud.IdSolicitud,

                IdMedico = medico.IdMedico,

                FechaCita = solicitud.FechaDeseada,

                HoraInicio = solicitud.HoraDeseada,

                HoraFin = solicitud.HoraDeseada
                    .AddMinutes(medico.DuracionCitaMin),

                IdEstadoCita = estadoCita.IdEstadoCita,

                Observaciones = solicitud.Motivo
            };


            _context.Citas.Add(cita);


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }




        // Cancelar solicitud
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return BadRequest("Debe ingresar un motivo de cancelación");
            }

            var cita = await _context.Citas
                .FirstOrDefaultAsync(x => x.IdCita == id);

            if (cita == null)
                return NotFound();

            var estadoCancelada = await _context.EstadosCita
                .FirstOrDefaultAsync(x => x.Nombre == "Cancelada");

            cita.IdEstadoCita = estadoCancelada.IdEstadoCita;

            cita.Observaciones = motivo;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}