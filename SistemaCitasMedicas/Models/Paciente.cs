using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class Paciente
    {
        [Key]
        public int IdPaciente { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? FechaNacimiento { get; set; }

        [StringLength(1)]
        public string? Sexo { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;

        public ICollection<SolicitudCita> Solicitudes { get; set; } = new List<SolicitudCita>();
    }
}