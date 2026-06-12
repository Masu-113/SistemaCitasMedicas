using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class SolicitudCita
    {
        [Key]
        public int IdSolicitud { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [Required]
        public int IdEspecialidad { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly FechaDeseada { get; set; }

        [Required]
        public TimeOnly HoraDeseada { get; set; }

        [Required]
        [StringLength(300)]
        public string Motivo { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        public int IdEstadoSolicitud { get; set; }

        [StringLength(500)]
        public string? ComentarioRespuesta { get; set; }

        [ForeignKey(nameof(IdPaciente))]
        public Paciente Paciente { get; set; } = null!;

        [ForeignKey(nameof(IdEspecialidad))]
        public Especialidad Especialidad { get; set; } = null!;

        [ForeignKey(nameof(IdEstadoSolicitud))]
        public EstadoSolicitud EstadoSolicitud { get; set; } = null!;

        public Cita? Cita { get; set; }

        public ICollection<HistorialSolicitud> Historiales { get; set; } = new List<HistorialSolicitud>();
    }
}