namespace GestorCitasAPI.DTOs
{
    public class CitaDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ServicioId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Notas { get; set; }
    }

    public class CrearCitaDTO
    {
        public int ClienteId { get; set; }
        public int ServicioId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public string? Notas { get; set; }
    }

    public class CitaDetalleDTO
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ServicioNombre { get; set; } = string.Empty;
        public string ProfesionalNombre { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }
}