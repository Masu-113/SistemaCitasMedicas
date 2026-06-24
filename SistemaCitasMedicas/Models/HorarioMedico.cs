using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class HorarioMedico
    {
        [Key]
        public int IdHorario { get; set; }

        [Required]
        public int IdMedico { get; set; }

        [ForeignKey(nameof(IdMedico))]
        public Medico Medico { get; set; } = null!;

        [Range(1, 7)]
        public byte DiaSemana { get; set; }

        [Required]
        public TimeOnly HoraInicio { get; set; }

        [Required]
        public TimeOnly HoraFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}