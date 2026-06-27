using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize(Roles = "Administrador,Paciente")]
    public class SolicitudCitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudCitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SolicitudCitas
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.SolicitudesCita
                .Include(s => s.Especialidad)
                .Include(s => s.EstadoSolicitud)
                .Include(s => s.Paciente)
                    .ThenInclude(p => p.Usuario)
                .AsQueryable();

            if (User.IsInRole("Paciente"))
            {
                if (int.TryParse(userId, out var userIdInt))
                {
                    query = query.Where(s => s.Paciente.Usuario.IdUsuario == userIdInt);
                }
                else
                {
                    query = query.Where(s => false);
                }
            }

            var solicitudes = await query.ToListAsync();
            return View(solicitudes);
        }

        // GET: SolicitudCitas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudCita = await _context.SolicitudesCita
                .Include(s => s.Especialidad)
                .Include(s => s.EstadoSolicitud)
                .Include(s => s.Paciente)
                    .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.IdSolicitud == id);
            if (solicitudCita == null)
            {
                return NotFound();
            }

            return View(solicitudCita);
        }

        // GET: SolicitudCitas/Create
        public async Task<IActionResult> Create()
        {
            ViewData["IdEspecialidad"] =
                new SelectList(_context.Especialidades, "IdEspecialidad", "Nombre");

            if (User.IsInRole("Paciente"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userId, out var userIdInt))
                {
                    ViewBag.ErrorMessage = "Usuario inválido";
                    return View();
                }

                var paciente = await _context.Pacientes
                    .Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.Usuario.IdUsuario == userIdInt);

                if (paciente == null)
                {
                    ViewBag.ErrorMessage = "No se encontró el paciente";
                    return View();
                }

                ViewData["PacienteNombre"] = paciente.Usuario.Nombre;
                ViewData["PacienteId"] = paciente.IdPaciente;
            }

            else if (User.IsInRole("Administrador"))
            {
                ViewData["IdPaciente"] =
                    new SelectList(
                        _context.Pacientes.Include(p => p.Usuario),
                        "IdPaciente",
                        "Usuario.Nombre"
                    );
            }

            return View();
        }

        // POST: SolicitudCitas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudCita solicitudCita)
        {
            try
            {
                ModelState.Remove("Paciente");
                ModelState.Remove("Especialidad");
                ModelState.Remove("EstadoSolicitud");

                if (!ModelState.IsValid)
                {
                    return View(solicitudCita);
                }

                // FORZAR valores del sistema
                solicitudCita.IdEstadoSolicitud = 1;
                solicitudCita.FechaSolicitud = DateTime.Now;

                _context.SolicitudesCita.Add(solicitudCita);
                await _context.SaveChangesAsync();

                // 🔥 HISTORIAL (CREACIÓN)
                _context.HistorialesSolicitud.Add(new HistorialSolicitud
                {
                    IdSolicitud = solicitudCita.IdSolicitud,
                    IdMedico = null,
                    Accion = "CREADA",
                    Comentario = "Solicitud creada por el paciente",
                    Fecha = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GUARDANDO:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                throw;
            }
        }

        // GET: SolicitudCitas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudCita = await _context.SolicitudesCita.FindAsync(id);
            if (solicitudCita == null)
            {
                return NotFound();
            }
            ViewData["IdEspecialidad"] = new SelectList(_context.Especialidades, "IdEspecialidad", "Nombre", solicitudCita.IdEspecialidad);
            ViewData["IdEstadoSolicitud"] = new SelectList(_context.EstadosSolicitud, "IdEstadoSolicitud", "Nombre", solicitudCita.IdEstadoSolicitud);
            ViewData["IdPaciente"] = new SelectList(_context.Pacientes, "IdPaciente", "IdPaciente", solicitudCita.IdPaciente);
            return View(solicitudCita);
        }

        // POST: SolicitudCitas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSolicitud,IdPaciente,IdEspecialidad,FechaDeseada,HoraDeseada,Motivo,FechaSolicitud,IdEstadoSolicitud,ComentarioRespuesta")] SolicitudCita solicitudCita)
        {
            if (id != solicitudCita.IdSolicitud)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitudCita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitudCitaExists(solicitudCita.IdSolicitud))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEspecialidad"] = new SelectList(_context.Especialidades, "IdEspecialidad", "Nombre", solicitudCita.IdEspecialidad);
            ViewData["IdEstadoSolicitud"] = new SelectList(_context.EstadosSolicitud, "IdEstadoSolicitud", "Nombre", solicitudCita.IdEstadoSolicitud);
            ViewData["IdPaciente"] = new SelectList(_context.Pacientes, "IdPaciente", "IdPaciente", solicitudCita.IdPaciente);
            return View(solicitudCita);
        }

        // GET: SolicitudCitas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudCita = await _context.SolicitudesCita
                .Include(s => s.Especialidad)
                .Include(s => s.EstadoSolicitud)
                .Include(s => s.Paciente)
                .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.IdSolicitud == id);
            if (solicitudCita == null)
            {
                return NotFound();
            }

            return View(solicitudCita);
        }

        // POST: SolicitudCitas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitudCita = await _context.SolicitudesCita.FindAsync(id);
            if (solicitudCita != null)
            {
                _context.SolicitudesCita.Remove(solicitudCita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudCitaExists(int id)
        {
            return _context.SolicitudesCita.Any(e => e.IdSolicitud == id);
        }
    }
}
