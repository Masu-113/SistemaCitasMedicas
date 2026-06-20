using Microsoft.EntityFrameworkCore;
using SistemaCitasMedicas.Models;

namespace SistemaCitasMedicas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Seguridad
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        // Personas
        public DbSet<Paciente> Pacientes { get; set; } 
        public DbSet<Medico> Medicos { get; set; }

        // Catálogos
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<EstadoSolicitud> EstadosSolicitud { get; set; }
        public DbSet<EstadoCita> EstadosCita { get; set; }

        // Horarios
        public DbSet<HorarioMedico> HorariosMedico { get; set; }

        // Proceso de citas
        public DbSet<SolicitudCita> SolicitudesCita { get; set; }
        public DbSet<Cita> Citas { get; set; }

        // Auditoría
        public DbSet<HistorialSolicitud> HistorialesSolicitud { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario -> Paciente (1:1)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Paciente)
                .WithOne(p => p.Usuario)
                .HasForeignKey<Paciente>(p => p.IdUsuario);

            // Usuario -> Médico (1:1)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Medico)
                .WithOne(m => m.Usuario)
                .HasForeignKey<Medico>(m => m.IdUsuario);

            // Solicitud -> Cita (1:1)
            modelBuilder.Entity<SolicitudCita>()
                .HasOne(s => s.Cita)
                .WithOne(c => c.Solicitud)
                .HasForeignKey<Cita>(c => c.IdSolicitud)
                .OnDelete(DeleteBehavior.Restrict);

            // Cita -> Medico
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Medico)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.IdMedico)
                .OnDelete(DeleteBehavior.Restrict);

            // Solicitud -> Paciente
            modelBuilder.Entity<SolicitudCita>()
                .HasOne(s => s.Paciente)
                .WithMany(p => p.Solicitudes)
                .HasForeignKey(s => s.IdPaciente)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único para correo
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // Índice único para licencia médica
            modelBuilder.Entity<Medico>()
                .HasIndex(m => m.NumeroLicencia)
                .IsUnique();

            // Índice para búsqueda de solicitudes por especialidad
            modelBuilder.Entity<SolicitudCita>()
                .HasIndex(s => s.IdEspecialidad);

            // Índice para búsqueda de solicitudes por estado
            modelBuilder.Entity<SolicitudCita>()
                .HasIndex(s => s.IdEstadoSolicitud);

            // Índice para agenda médica
            modelBuilder.Entity<Cita>()
                .HasIndex(c => new
                {
                    c.IdMedico,
                    c.FechaCita
                });

            // Índice para horarios médicos
            modelBuilder.Entity<HorarioMedico>()
                .HasIndex(h => new
                {
                    h.IdMedico,
                    h.DiaSemana
                });

            // Restricción para evitar horarios duplicados
            modelBuilder.Entity<HorarioMedico>()
                .HasIndex(h => new
                {
                    h.IdMedico,
                    h.DiaSemana,
                    h.HoraInicio,
                    h.HoraFin
                })
                .IsUnique();

            // Establece que el campo cedula sea Unico
            modelBuilder.Entity<Paciente>()
                .HasIndex(p => p.Cedula)
                .IsUnique();
        }
    }
}