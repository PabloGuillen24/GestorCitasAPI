namespace GestorCitasAPI.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ServicioId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Notas { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
        public Servicio Servicio { get; set; } = null!;
        public Profesional Profesional { get; set; } = null!;
    }
}