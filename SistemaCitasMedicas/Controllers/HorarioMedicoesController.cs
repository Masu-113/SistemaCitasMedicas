using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize(Roles = "Administrador,Medico")]
    public class HorarioMedicoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorarioMedicoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO
        public async Task<IActionResult> Index()
        {
            var horariosQuery = _context.HorariosMedico
                .Include(h => h.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(h => h.Medico)
                    .ThenInclude(m => m.Especialidad)
                .AsQueryable();

            // Médico: solo sus propios horarios
            if (User.IsInRole("Medico"))
            {
                var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(idUsuarioClaim))
                    return BadRequest("Usuario no identificado");


                int idUsuario = int.Parse(idUsuarioClaim);


                horariosQuery = horariosQuery
                    .Where(h =>
                        h.Medico.IdUsuario == idUsuario &&
                        h.Activo == true
                    );
            }

            // Administrador: ve todos
            var horarios = await horariosQuery
                .OrderBy(h => h.IdMedico)
                .ThenBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();


            return View(horarios);
        }

        // DETALLE
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var horario = await _context.HorariosMedico
                .Include(h => h.Medico)
                    .ThenInclude(m => m.Usuario)
                .Include(h => h.Medico)
                    .ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(m => m.IdHorario == id);

            if (horario == null)
                return NotFound();

            if (!User.IsInRole("Administrador"))
            {
                var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (horario.Medico?.IdUsuario != idUsuario)
                    return Forbid();
            }

            return View(horario);
        }

        // CREATE - GET
        public IActionResult Create()
        {
            ViewData["IdMedico"] = new SelectList(
                _context.Medicos
                    .Include(m => m.Usuario)
                    .Include(m => m.Especialidad)
                    .Select(m => new
                    {
                        m.IdMedico,
                        Nombre = m.Usuario.Nombre + " " + m.Usuario.Apellido
                    }),
                "IdMedico",
                "Nombre"
            );

            return View(new HorarioMedico());
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(HorarioMedico horario)
        {
            // Ignorar validación de navegación
            ModelState.Remove("Medico");


            if (!ModelState.IsValid)
            {
                ViewData["IdMedico"] = new SelectList(
                    _context.Medicos
                        .Include(m => m.Usuario)
                        .Select(m => new
                        {
                            m.IdMedico,
                            Nombre = m.Usuario.Nombre + " " + m.Usuario.Apellido
                        }),
                    "IdMedico",
                    "Nombre",
                    horario.IdMedico
                );


                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Campo = x.Key,
                        Errores = x.Value.Errors.Select(e => e.ErrorMessage)
                    });


                Console.WriteLine(
                    System.Text.Json.JsonSerializer.Serialize(errores)
                );


                return View(horario);
            }


            _context.HorariosMedico.Add(horario);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, HorarioMedico horario)
        {
            if (id != horario.IdHorario)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["IdMedico"] = new SelectList(_context.Medicos, "IdMedico", "IdMedico", horario.IdMedico);
                return View(horario);
            }

            try
            {
                _context.Update(horario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HorarioExists(horario.IdHorario))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // DELETE - GET
        // DELETE - GET
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var horario = await _context.HorariosMedico
                .Include(h => h.Medico)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(h => h.IdHorario == id);


            if (horario == null)
                return NotFound();


            return View(horario);
        }



        // DELETE - POST (SOFT DELETE)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var horario = await _context.HorariosMedico
                .FirstOrDefaultAsync(h => h.IdHorario == id);

            if (horario == null)
                return NotFound();

            // Cambiar estado activo a inactivo
            horario.Activo = false;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HorarioExists(int id)
        {
            return _context.HorariosMedico.Any(e => e.IdHorario == id);
        }
    }
}