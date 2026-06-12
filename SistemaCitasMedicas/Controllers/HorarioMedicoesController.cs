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
    public class HorarioMedicoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorarioMedicoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HorarioMedicoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.HorariosMedico.ToListAsync());
        }

        // GET: HorarioMedicoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horarioMedico = await _context.HorariosMedico
                .FirstOrDefaultAsync(m => m.IdHorario == id);
            if (horarioMedico == null)
            {
                return NotFound();
            }

            return View(horarioMedico);
        }

        // GET: HorarioMedicoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HorarioMedicoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdHorario,IdMedico,DiaSemana,HoraInicio,HoraFin,Activo")] HorarioMedico horarioMedico)
        {
            if (ModelState.IsValid)
            {
                _context.Add(horarioMedico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(horarioMedico);
        }

        // GET: HorarioMedicoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horarioMedico = await _context.HorariosMedico.FindAsync(id);
            if (horarioMedico == null)
            {
                return NotFound();
            }
            return View(horarioMedico);
        }

        // POST: HorarioMedicoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdHorario,IdMedico,DiaSemana,HoraInicio,HoraFin,Activo")] HorarioMedico horarioMedico)
        {
            if (id != horarioMedico.IdHorario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(horarioMedico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HorarioMedicoExists(horarioMedico.IdHorario))
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
            return View(horarioMedico);
        }

        // GET: HorarioMedicoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horarioMedico = await _context.HorariosMedico
                .FirstOrDefaultAsync(m => m.IdHorario == id);
            if (horarioMedico == null)
            {
                return NotFound();
            }

            return View(horarioMedico);
        }

        // POST: HorarioMedicoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var horarioMedico = await _context.HorariosMedico.FindAsync(id);
            if (horarioMedico != null)
            {
                _context.HorariosMedico.Remove(horarioMedico);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HorarioMedicoExists(int id)
        {
            return _context.HorariosMedico.Any(e => e.IdHorario == id);
        }
    }
}
