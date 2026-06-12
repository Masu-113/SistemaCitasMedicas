using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class Medico
    {
        [Key]
        public int IdMedico { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdEspecialidad { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroLicencia { get; set; } = string.Empty;

        [Range(1, 240)]
        public int DuracionCitaMin { get; set; } = 30;

        public bool Activo { get; set; } = true;

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(IdEspecialidad))]
        public Especialidad Especialidad { get; set; } = null!;

        public ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();

        public ICollection<HistorialSolicitud> Historiales { get; set; } = new List<HistorialSolicitud>();
    }
}