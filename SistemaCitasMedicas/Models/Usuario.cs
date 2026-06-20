using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCitasMedicas.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string SegundoNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Apellidos obligatorios")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El IdRol es obligatorio")]
        public int IdRol { get; set; }

        [ForeignKey(nameof(IdRol))]
        public Rol? Rol { get; set; }

        public Paciente? Paciente { get; set; }

        public Medico? Medico { get; set; }
        public string? Cedula { get; set; }
    }
}