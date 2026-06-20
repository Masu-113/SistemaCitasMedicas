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
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Usuarios.Include(u => u.Rol);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["IdRol"] =
                new SelectList(_context.Roles, "IdRol", "Nombre");

            ViewData["IdEspecialidad"] =
                new SelectList(_context.Especialidades,
                               "IdEspecialidad",
                               "Nombre");

            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    SegundoNombre = model.SegundoNombre,
                    Apellido = model.Apellido,
                    Correo = model.Correo,
                    PasswordHash = model.PasswordHash,
                    Telefono = model.Telefono,
                    Activo = model.Activo,
                    FechaRegistro = DateTime.Now,
                    IdRol = model.IdRol
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // MÉDICO
                if (model.IdEspecialidad.HasValue &&
                    !string.IsNullOrWhiteSpace(model.NumeroLicencia))
                {
                    var medico = new Medico 
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdEspecialidad = model.IdEspecialidad.Value,
                        NumeroLicencia = model.NumeroLicencia,
                        DuracionCitaMin = model.DuracionCitaMin ?? 30,
                        Activo = true
                    };

                    _context.Medicos.Add(medico);
                }

                if (!ModelState.IsValid)
                {
                    // Aquí puedes ver los errores específicos
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage); // O registra el error en un log
                    }
                    return View(model); // Retorna a la vista con el modelo para mostrar los errores
                }


                // PACIENTE
                if (model.FechaNacimiento.HasValue)
                {
                    var paciente = new Paciente
                    {
                        IdUsuario = usuario.IdUsuario,
                        Cedula = model.Cedula,
                        FechaNacimiento = model.FechaNacimiento,
                        Sexo = model.Sexo,
                        Direccion = model.Direccion
                    };

                    _context.Pacientes.Add(paciente);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdRol"] =
                new SelectList(_context.Roles,
                               "IdRol",
                               "Nombre",
                               model.IdRol);

            ViewData["IdEspecialidad"] =
                new SelectList(_context.Especialidades,
                               "IdEspecialidad",
                               "Nombre",
                               model.IdEspecialidad);

            return View(model);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Nombre,Apellido,Correo,PasswordHash,Telefono,Activo,FechaRegistro,IdRol")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
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
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
