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
    public class HistorialSolicitudsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialSolicitudsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HistorialSolicituds
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.HistorialesSolicitud.Include(h => h.Solicitud);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: HistorialSolicituds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialSolicitud = await _context.HistorialesSolicitud
                .Include(h => h.Solicitud)
                .FirstOrDefaultAsync(m => m.IdHistorial == id);
            if (historialSolicitud == null)
            {
                return NotFound();
            }

            return View(historialSolicitud);
        }

        // GET: HistorialSolicituds/Create
        public IActionResult Create()
        {
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo");
            return View();
        }

        // POST: HistorialSolicituds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdHistorial,IdSolicitud,IdMedico,Accion,Comentario,Fecha")] HistorialSolicitud historialSolicitud)
        {
            if (ModelState.IsValid)
            {
                _context.Add(historialSolicitud);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo", historialSolicitud.IdSolicitud);
            return View(historialSolicitud);
        }

        // GET: HistorialSolicituds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialSolicitud = await _context.HistorialesSolicitud.FindAsync(id);
            if (historialSolicitud == null)
            {
                return NotFound();
            }
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo", historialSolicitud.IdSolicitud);
            return View(historialSolicitud);
        }

        // POST: HistorialSolicituds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdHistorial,IdSolicitud,IdMedico,Accion,Comentario,Fecha")] HistorialSolicitud historialSolicitud)
        {
            if (id != historialSolicitud.IdHistorial)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(historialSolicitud);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HistorialSolicitudExists(historialSolicitud.IdHistorial))
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
            ViewData["IdSolicitud"] = new SelectList(_context.SolicitudesCita, "IdSolicitud", "Motivo", historialSolicitud.IdSolicitud);
            return View(historialSolicitud);
        }

        // GET: HistorialSolicituds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialSolicitud = await _context.HistorialesSolicitud
                .Include(h => h.Solicitud)
                .FirstOrDefaultAsync(m => m.IdHistorial == id);
            if (historialSolicitud == null)
            {
                return NotFound();
            }

            return View(historialSolicitud);
        }

        // POST: HistorialSolicituds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var historialSolicitud = await _context.HistorialesSolicitud.FindAsync(id);
            if (historialSolicitud != null)
            {
                _context.HistorialesSolicitud.Remove(historialSolicitud);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HistorialSolicitudExists(int id)
        {
            return _context.HistorialesSolicitud.Any(e => e.IdHistorial == id);
        }
    }
}
