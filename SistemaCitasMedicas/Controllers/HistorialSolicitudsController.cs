using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Data;
using SistemaCitasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaCitasMedicas.Controllers
{
    [Authorize]
    public class HistorialSolicitudesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialSolicitudesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📌 SOLO PARA CONSULTAR HISTORIAL DE UNA SOLICITUD
        public async Task<IActionResult> PorSolicitud(int idSolicitud)
        {
            var historial = await _context.HistorialesSolicitud
                .Include(h => h.Solicitud)
                .Include(h => h.Medico)
                    .ThenInclude(m => m.Usuario)
                .Where(h => h.IdSolicitud == idSolicitud)
                .OrderByDescending(h => h.Fecha)
                .ToListAsync();

            ViewBag.IdSolicitud = idSolicitud;

            return View(historial);
        }

        // ---------------------------------------------------
        // 📌 MÉTODO CENTRAL: REGISTRAR EVENTO (REUTILIZABLE)
        // ---------------------------------------------------
        public async Task Registrar(int idSolicitud, string accion, int? idMedico = null, string? comentario = null)
        {
            var historial = new HistorialSolicitud
            {
                IdSolicitud = idSolicitud,
                IdMedico = idMedico,
                Accion = accion,
                Comentario = comentario,
                Fecha = DateTime.Now
            };

            _context.HistorialesSolicitud.Add(historial);
            await _context.SaveChangesAsync();
        }
    }
}