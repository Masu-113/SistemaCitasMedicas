using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdSolicitud { get; set; }

        [Required]
        public int IdMedico { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly FechaCita { get; set; }

        [Required]
        public TimeOnly HoraInicio { get; set; }

        [Required]
        public TimeOnly HoraFin { get; set; }

        [Required]
        public int IdEstadoCita { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [ForeignKey(nameof(IdSolicitud))]
        public SolicitudCita Solicitud { get; set; } = null!;

        [ForeignKey(nameof(IdMedico))]
        public Medico Medico { get; set; } = null!;

        [ForeignKey(nameof(IdEstadoCita))]
        public EstadoCita EstadoCita { get; set; } = null!;
    }
}