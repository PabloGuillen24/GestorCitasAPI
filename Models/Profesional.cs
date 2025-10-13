namespace GestorCitasAPI.Models
{
    public class Profesional
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool Activo { get; set; } = true;
        
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}