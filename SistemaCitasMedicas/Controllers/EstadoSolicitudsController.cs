using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EstadoSolicitudsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadoSolicitudsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EstadoSolicituds
        public async Task<IActionResult> Index()
        {
            return View(await _context.EstadosSolicitud.ToListAsync());
        }

        // GET: EstadoSolicituds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoSolicitud = await _context.EstadosSolicitud
                .FirstOrDefaultAsync(m => m.IdEstadoSolicitud == id);
            if (estadoSolicitud == null)
            {
                return NotFound();
            }

            return View(estadoSolicitud);
        }

        // GET: EstadoSolicituds/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EstadoSolicituds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEstadoSolicitud,Nombre")] EstadoSolicitud estadoSolicitud)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estadoSolicitud);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(estadoSolicitud);
        }

        // GET: EstadoSolicituds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoSolicitud = await _context.EstadosSolicitud.FindAsync(id);
            if (estadoSolicitud == null)
            {
                return NotFound();
            }
            return View(estadoSolicitud);
        }

        // POST: EstadoSolicituds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEstadoSolicitud,Nombre")] EstadoSolicitud estadoSolicitud)
        {
            if (id != estadoSolicitud.IdEstadoSolicitud)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(estadoSolicitud);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstadoSolicitudExists(estadoSolicitud.IdEstadoSolicitud))
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
            return View(estadoSolicitud);
        }

        // GET: EstadoSolicituds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoSolicitud = await _context.EstadosSolicitud
                .FirstOrDefaultAsync(m => m.IdEstadoSolicitud == id);
            if (estadoSolicitud == null)
            {
                return NotFound();
            }

            return View(estadoSolicitud);
        }

        // POST: EstadoSolicituds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoSolicitud = await _context.EstadosSolicitud.FindAsync(id);
            if (estadoSolicitud != null)
            {
                _context.EstadosSolicitud.Remove(estadoSolicitud);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EstadoSolicitudExists(int id)
        {
            return _context.EstadosSolicitud.Any(e => e.IdEstadoSolicitud == id);
        }
    }
}
