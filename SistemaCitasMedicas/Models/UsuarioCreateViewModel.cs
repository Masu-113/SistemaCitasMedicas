namespace SistemaCitasMedicas.Models
{
    public class UsuarioCreateViewModel
    {
        // Usuario
        public required string Nombre { get; set; }
        public required string SegundoNombre { get; set; }
        public required string Apellido { get; set; }
        public required string Correo { get; set; }
        public required string PasswordHash { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public int IdRol { get; set; }

        // Médico
        public int? IdEspecialidad { get; set; }
        public string? NumeroLicencia { get; set; }
        public int? DuracionCitaMin { get; set; }

        // Paciente
        public DateOnly? FechaNacimiento { get; set; }
        public required string Cedula { get; set; }
        public string? Sexo { get; set; }
        public string? Direccion { get; set; }
    }
}
