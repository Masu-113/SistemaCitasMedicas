using System.ComponentModel.DataAnnotations;

namespace SistemaCitasMedicas.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
