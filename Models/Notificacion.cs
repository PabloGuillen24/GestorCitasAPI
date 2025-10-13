namespace GestorCitasAPI.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Destinatario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Enviado";
        
        public Cita Cita { get; set; } = null!;
    }
}