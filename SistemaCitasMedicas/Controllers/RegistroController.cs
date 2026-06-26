using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;

namespace SistemaCitasMedicas.Controllers
{
    public class RegistroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistroController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new UsuarioCreateViewModel
            {
                Nombre = string.Empty,
                SegundoNombre = string.Empty,
                Apellido = string.Empty,
                Correo = string.Empty,
                PasswordHash = string.Empty,
                Cedula = string.Empty,
                Activo = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UsuarioCreateViewModel model)
        {
            var rolPaciente = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == "Paciente");

            if (rolPaciente == null)
            {
                ModelState.AddModelError("", "No existe el rol Paciente en el sistema.");
                return View(model);
            }

            model.IdRol = rolPaciente.IdRol;

            ModelState.Remove(nameof(model.IdRol));
            ModelState.Remove(nameof(model.NumeroLicencia));
            ModelState.Remove(nameof(model.IdEspecialidad));
            ModelState.Remove(nameof(model.DuracionCitaMin));

            if (string.IsNullOrWhiteSpace(model.Cedula))
            {
                ModelState.AddModelError(nameof(model.Cedula), "La cédula es obligatoria.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == model.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError(nameof(model.Correo), "Este correo ya está registrado.");
                return View(model);
            }

            var cedulaLimpia = model.Cedula.Trim();

            var cedulaExiste = await _context.Pacientes
                .AnyAsync(p => p.Cedula == cedulaLimpia);

            if (cedulaExiste)
            {
                ModelState.AddModelError(nameof(model.Cedula), "Esta cédula ya está registrada.");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    SegundoNombre = model.SegundoNombre,
                    Apellido = model.Apellido,
                    Correo = model.Correo,
                    PasswordHash = model.PasswordHash,
                    Telefono = model.Telefono,
                    Activo = true,
                    FechaRegistro = DateTime.Now,
                    IdRol = rolPaciente.IdRol
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var paciente = new Paciente
                {
                    IdUsuario = usuario.IdUsuario,
                    Cedula = cedulaLimpia,
                    FechaNacimiento = model.FechaNacimiento,
                    Sexo = model.Sexo,
                    Direccion = model.Direccion
                };

                _context.Pacientes.Add(paciente);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error al registrar usuario: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == model.Correo);

            if (usuario == null || usuario.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            if (!usuario.Activo)
            {
                ModelState.AddModelError("", "El usuario está inactivo.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }
    }
}