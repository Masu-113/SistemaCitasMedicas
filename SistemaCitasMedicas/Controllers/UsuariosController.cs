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
            if (!ModelState.IsValid)
            {
                CargarCombos(model);
                return View(model);
            }

            // ✔ VALIDACIÓN DUPLICADOS
            var correoExiste = await _context.Usuarios
                .AnyAsync(x => x.Correo == model.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado");
                CargarCombos(model);
                return View(model);
            }

            var cedulaExiste = await _context.Pacientes
                .AnyAsync(x => x.Cedula == model.Cedula);

            if (cedulaExiste)
            {
                ModelState.AddModelError("Cedula", "Esta cédula ya está registrada");
                CargarCombos(model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✔ USUARIO
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

                // ✔ MÉDICO
                if (model.IdEspecialidad.HasValue &&
                    !string.IsNullOrWhiteSpace(model.NumeroLicencia))
                {
                    _context.Medicos.Add(new Medico
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdEspecialidad = model.IdEspecialidad.Value,
                        NumeroLicencia = model.NumeroLicencia,
                        DuracionCitaMin = model.DuracionCitaMin ?? 30,
                        Activo = true
                    });
                }

                // ✔ PACIENTE
                if (model.FechaNacimiento.HasValue)
                {
                    _context.Pacientes.Add(new Paciente
                    {
                        IdUsuario = usuario.IdUsuario,
                        Cedula = model.Cedula,
                        FechaNacimiento = model.FechaNacimiento,
                        Sexo = model.Sexo,
                        Direccion = model.Direccion
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error al crear usuario: " + ex.Message);
                CargarCombos(model);
                return View(model);
            }
        }

        private void CargarCombos(UsuarioCreateViewModel model)
        {
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", model.IdRol);

            ViewData["IdEspecialidad"] = new SelectList(_context.Especialidades, "IdEspecialidad", "Nombre", model.IdEspecialidad);
        }

        // GET: Usuarios/Edit/5
        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return NotFound();

            var model = new UsuarioCreateViewModel
            {
                Nombre = usuario.Nombre,
                SegundoNombre = usuario.SegundoNombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                PasswordHash = usuario.PasswordHash,
                Telefono = usuario.Telefono,
                Activo = usuario.Activo,
                IdRol = usuario.IdRol,
                Cedula = usuario.Cedula ?? string.Empty // Ensure required property is set
                // si quieres mapear datos de médico/paciente, aquí
            };

            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuario.IdRol);
            ViewData["IdEspecialidad"] = new SelectList(_context.Especialidades, "IdEspecialidad", "Nombre");

            return View(model);
        }


        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioCreateViewModel model)
        {
            if (id != model.IdUsuario) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", model.IdRol);
                return View(model);
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.SegundoNombre = model.SegundoNombre;
            usuario.Apellido = model.Apellido;
            usuario.Correo = model.Correo;
            usuario.PasswordHash = model.PasswordHash;
            usuario.Telefono = model.Telefono;
            usuario.Activo = model.Activo;
            usuario.IdRol = model.IdRol;

            // No tocamos FechaRegistro ni datos de médico/paciente

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
