using System.ComponentModel.DataAnnotations;

namespace SistemaCitasMedicas.Models
{
    public class EstadoCita
    {
        [Key]
        public int IdEstadoCita { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}