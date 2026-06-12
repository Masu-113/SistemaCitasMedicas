using System.ComponentModel.DataAnnotations;

namespace SistemaCitasMedicas.Models
{
    public class HorarioMedico
    {
        [Key]
        public int IdHorario { get; set; }

        [Required]
        public int IdMedico { get; set; }

        [Range(1, 7)]
        public byte DiaSemana { get; set; }

        [Required]
        public TimeOnly HoraInicio { get; set; }

        [Required]
        public TimeOnly HoraFin { get; set; }

        public bool Activo { get; set; } = true;

        public Medico Medico { get; set; } = null!;
    }
}