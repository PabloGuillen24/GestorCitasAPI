namespace GestorCitasAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}