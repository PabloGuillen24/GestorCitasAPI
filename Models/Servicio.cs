namespace GestorCitasAPI.Models
{
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
        
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}