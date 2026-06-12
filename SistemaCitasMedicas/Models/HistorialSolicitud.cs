using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class HistorialSolicitud
    {
        [Key]
        public int IdHistorial { get; set; }

        [Required]
        public int IdSolicitud { get; set; }

        public int? IdMedico { get; set; }

        [Required]
        [StringLength(50)]
        public string Accion { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Comentario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [ForeignKey(nameof(IdSolicitud))]
        public SolicitudCita Solicitud { get; set; } = null!;

        public Medico? Medico { get; set; }
    }
}