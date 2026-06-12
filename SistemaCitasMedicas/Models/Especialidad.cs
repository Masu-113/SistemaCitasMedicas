using System.ComponentModel.DataAnnotations;

namespace SistemaCitasMedicas.Models
{
    public class Especialidad
    {
        [Key]
        public int IdEspecialidad { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        public ICollection<Medico> Medicos { get; set; } = new List<Medico>();

        public ICollection<SolicitudCita> Solicitudes { get; set; } = new List<SolicitudCita>();
    }
}