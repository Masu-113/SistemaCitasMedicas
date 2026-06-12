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
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Citas.Include(c => c.EstadoCita).Include(c => c.Medico).Include(c => c.Solicitud);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas
                .Include(c => c.EstadoCita)
                .Include(c => c.Medico)
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(m => m.IdCita == id);
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // GET: Citas/Create
        public IActionResult Create()
        {
            ViewData["IdEstadoCita"] = new SelectList(_context.EstadosCita, "IdEstadoCita", "Nombre");
            ViewData["IdMedico"] = new SelectList(_context.Medicos, "IdMedico", "NumeroLicencia");
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo");
            return View();
        }

        // POST: Citas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCita,IdSolicitud,IdMedico,FechaCita,HoraInicio,HoraFin,IdEstadoCita,Observaciones,FechaCreacion")] Cita cita)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEstadoCita"] = new SelectList(_context.EstadosCita, "IdEstadoCita", "Nombre", cita.IdEstadoCita);
            ViewData["IdMedico"] = new SelectList(_context.Medicos, "IdMedico", "NumeroLicencia", cita.IdMedico);
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo", cita.IdSolicitud);
            return View(cita);
        }

        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                return NotFound();
            }
            ViewData["IdEstadoCita"] = new SelectList(_context.EstadosCita, "IdEstadoCita", "Nombre", cita.IdEstadoCita);
            ViewData["IdMedico"] = new SelectList(_context.Medicos, "IdMedico", "NumeroLicencia", cita.IdMedico);
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo", cita.IdSolicitud);
            return View(cita);
        }

        // POST: Citas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCita,IdSolicitud,IdMedico,FechaCita,HoraInicio,HoraFin,IdEstadoCita,Observaciones,FechaCreacion")] Cita cita)
        {
            if (id != cita.IdCita)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitaExists(cita.IdCita))
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
            ViewData["IdEstadoCita"] = new SelectList(_context.EstadosCita, "IdEstadoCita", "Nombre", cita.IdEstadoCita);
            ViewData["IdMedico"] = new SelectList(_context.Medicos, "IdMedico", "NumeroLicencia", cita.IdMedico);
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo", cita.IdSolicitud);
            return View(cita);
        }

        // GET: Citas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas
                .Include(c => c.EstadoCita)
                .Include(c => c.Medico)
                .Include(c => c.Solicitud)
                .FirstOrDefaultAsync(m => m.IdCita == id);
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(e => e.IdCita == id);
        }
    }
}
