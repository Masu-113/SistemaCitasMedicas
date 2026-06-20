using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;

namespace SistemaCitasMedicas.Controllers
{
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
            var applicationDbContext = _context.SolicitudesCita.Include(s => s.Especialidad).Include(s => s.EstadoSolicitud).Include(s => s.Paciente);
            return View(await applicationDbContext.ToListAsync());
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
                .FirstOrDefaultAsync(m => m.IdSolicitud == id);
            if (solicitudCita == null)
            {
                return NotFound();
            }

            return View(solicitudCita);
        }

        // GET: SolicitudCitas/Create
        public IActionResult Create()
        {
            ViewData["IdEspecialidad"] = new SelectList(_context.Especialidades, "IdEspecialidad", "Nombre");
            ViewData["IdPaciente"] = new SelectList(_context.Pacientes, "IdPaciente", "IdPaciente");
            return View();
        }

        // POST: SolicitudCitas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudCita solicitudCita)
        {
            Console.WriteLine("ENTRO AL CREATE POST");

            try
            {
                // 🔥 FIX: evitar que navegación rompa ModelState
                ModelState.Remove("Paciente");
                ModelState.Remove("Especialidad");
                ModelState.Remove("EstadoSolicitud");

                // 🔥 FIX: si hay otros campos raros, ignorar validación de relaciones
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState)
                    {
                        foreach (var e in error.Value.Errors)
                        {
                            Console.WriteLine($"CAMPO: {error.Key} ERROR: {e.ErrorMessage}");
                        }
                    }

                    ViewData["IdEspecialidad"] = new SelectList(
                        _context.Especialidades,
                        "IdEspecialidad",
                        "Nombre",
                        solicitudCita.IdEspecialidad
                    );

                    ViewData["IdPaciente"] = new SelectList(
                        _context.Pacientes,
                        "IdPaciente",
                        "IdPaciente",
                        solicitudCita.IdPaciente
                    );

                    return View(solicitudCita);
                }

                // 🔥 FORZAR valores del sistema
                solicitudCita.IdEstadoSolicitud = 1;
                solicitudCita.FechaSolicitud = DateTime.Now;

                Console.WriteLine("PACIENTE: " + solicitudCita.IdPaciente);
                Console.WriteLine("ESPECIALIDAD: " + solicitudCita.IdEspecialidad);
                Console.WriteLine("FECHA: " + solicitudCita.FechaDeseada);
                Console.WriteLine("HORA: " + solicitudCita.HoraDeseada);
                Console.WriteLine("MOTIVO: " + solicitudCita.Motivo);
                Console.WriteLine("ESTADO: " + solicitudCita.IdEstadoSolicitud);

                // 🔥 INSERT REAL
                _context.SolicitudesCita.Add(solicitudCita);
                await _context.SaveChangesAsync();

                Console.WriteLine("GUARDADO EXITOSO");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GUARDANDO:");
                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER:");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
