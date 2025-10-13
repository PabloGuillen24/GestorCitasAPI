using Microsoft.EntityFrameworkCore;
using GestorCitasAPI.Models;

namespace GestorCitasAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Citas)
                .HasForeignKey(c => c.ClienteId);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Servicio)
                .WithMany(s => s.Citas)
                .HasForeignKey(c => c.ServicioId);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Profesional)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.ProfesionalId);

            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Cita)
                .WithMany()
                .HasForeignKey(n => n.CitaId);
        }
    }
}