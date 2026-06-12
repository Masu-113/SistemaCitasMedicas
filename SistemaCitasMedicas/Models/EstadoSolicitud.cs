using System.ComponentModel.DataAnnotations;

namespace SistemaCitasMedicas.Models
{
    public class EstadoSolicitud
    {
        [Key]
        public int IdEstadoSolicitud { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<SolicitudCita> Solicitudes { get; set; } = new List<SolicitudCita>();
    }
}