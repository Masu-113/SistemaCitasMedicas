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
    [Authorize]
    public class EstadoCitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadoCitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EstadoCitas
        public async Task<IActionResult> Index()
        {
            return View(await _context.EstadosCita.ToListAsync());
        }

        // GET: EstadoCitas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoCita = await _context.EstadosCita
                .FirstOrDefaultAsync(m => m.IdEstadoCita == id);
            if (estadoCita == null)
            {
                return NotFound();
            }

            return View(estadoCita);
        }

        // GET: EstadoCitas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EstadoCitas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdEstadoCita,Nombre")] EstadoCita estadoCita)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estadoCita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(estadoCita);
        }

        // GET: EstadoCitas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoCita = await _context.EstadosCita.FindAsync(id);
            if (estadoCita == null)
            {
                return NotFound();
            }
            return View(estadoCita);
        }

        // POST: EstadoCitas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdEstadoCita,Nombre")] EstadoCita estadoCita)
        {
            if (id != estadoCita.IdEstadoCita)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(estadoCita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstadoCitaExists(estadoCita.IdEstadoCita))
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
            return View(estadoCita);
        }

        // GET: EstadoCitas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estadoCita = await _context.EstadosCita
                .FirstOrDefaultAsync(m => m.IdEstadoCita == id);
            if (estadoCita == null)
            {
                return NotFound();
            }

            return View(estadoCita);
        }

        // POST: EstadoCitas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estadoCita = await _context.EstadosCita.FindAsync(id);
            if (estadoCita != null)
            {
                _context.EstadosCita.Remove(estadoCita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EstadoCitaExists(int id)
        {
            return _context.EstadosCita.Any(e => e.IdEstadoCita == id);
        }
    }
}
