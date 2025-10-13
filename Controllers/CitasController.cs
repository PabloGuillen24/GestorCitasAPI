using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorCitasAPI.Data;
using GestorCitasAPI.Models;
using GestorCitasAPI.DTOs;
using AutoMapper;

namespace GestorCitasAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CitasController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CitaDetalleDTO>>> GetCitas(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? profesionalId = null)
        {
            var query = _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Profesional)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(c => c.FechaHoraInicio >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(c => c.FechaHoraInicio <= fechaFin.Value);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(c => c.Estado == estado);
            }

            if (profesionalId.HasValue)
            {
                query = query.Where(c => c.ProfesionalId == profesionalId.Value);
            }

            var citas = await query.OrderBy(c => c.FechaHoraInicio).ToListAsync();
            return Ok(_mapper.Map<List<CitaDetalleDTO>>(citas));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CitaDetalleDTO>> GetCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Profesional)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            return _mapper.Map<CitaDetalleDTO>(cita);
        }

        [HttpPost]
        public async Task<ActionResult<CitaDTO>> PostCita(CrearCitaDTO crearCitaDTO)
        {
            var servicio = await _context.Servicios.FindAsync(crearCitaDTO.ServicioId);
            if (servicio == null)
            {
                return BadRequest("Servicio no encontrado");
            }

            var fechaHoraFin = crearCitaDTO.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);

            var conflicto = await _context.Citas
                .Where(c => c.ProfesionalId == crearCitaDTO.ProfesionalId)
                .Where(c => c.Estado != "Cancelada")
                .Where(c => (crearCitaDTO.FechaHoraInicio >= c.FechaHoraInicio && crearCitaDTO.FechaHoraInicio < c.FechaHoraFin) ||
                           (fechaHoraFin > c.FechaHoraInicio && fechaHoraFin <= c.FechaHoraFin) ||
                           (crearCitaDTO.FechaHoraInicio <= c.FechaHoraInicio && fechaHoraFin >= c.FechaHoraFin))
                .AnyAsync();

            if (conflicto)
            {
                return BadRequest("El profesional no está disponible en ese horario");
            }

            var cita = _mapper.Map<Cita>(crearCitaDTO);
            cita.FechaHoraFin = fechaHoraFin;

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            await CrearNotificacionConfirmacion(cita);

            return CreatedAtAction(nameof(GetCita), new { id = cita.Id }, 
                _mapper.Map<CitaDTO>(cita));
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstadoCita(int id, [FromBody] string nuevoEstado)
        {
            var estadosValidos = new[] { "Pendiente", "Confirmada", "Completada", "Cancelada", "NoShow" };
            
            if (!estadosValidos.Contains(nuevoEstado))
            {
                return BadRequest("Estado no válido");
            }

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Profesional)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            cita.Estado = nuevoEstado;
            cita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await CrearNotificacionCambioEstado(cita);

            return NoContent();
        }

        [HttpGet("disponibilidad")]
        public async Task<ActionResult> GetDisponibilidad(
            [FromQuery] int profesionalId,
            [FromQuery] DateTime fecha,
            [FromQuery] int servicioId)
        {
            var servicio = await _context.Servicios.FindAsync(servicioId);
            if (servicio == null)
            {
                return BadRequest("Servicio no encontrado");
            }

            var inicioDia = fecha.Date;
            var finDia = fecha.Date.AddDays(1);

            var citasOcupadas = await _context.Citas
                .Where(c => c.ProfesionalId == profesionalId)
                .Where(c => c.FechaHoraInicio >= inicioDia && c.FechaHoraInicio < finDia)
                .Where(c => c.Estado != "Cancelada")
                .Select(c => new { c.FechaHoraInicio, c.FechaHoraFin })
                .ToListAsync();

            var horariosDisponibles = new List<DateTime>();
            var horaInicio = fecha.Date.AddHours(9); 
            var horaFin = fecha.Date.AddHours(18);   

            for (var hora = horaInicio; hora < horaFin; hora = hora.AddMinutes(30))
            {
                var horaFinPropuesta = hora.AddMinutes(servicio.DuracionMinutos);
                
                if (horaFinPropuesta > horaFin)
                    continue;

                var disponible = !citasOcupadas.Any(c => 
                    (hora >= c.FechaHoraInicio && hora < c.FechaHoraFin) ||
                    (horaFinPropuesta > c.FechaHoraInicio && horaFinPropuesta <= c.FechaHoraFin) ||
                    (hora <= c.FechaHoraInicio && horaFinPropuesta >= c.FechaHoraFin));

                if (disponible)
                {
                    horariosDisponibles.Add(hora);
                }
            }

            return Ok(new { 
                ProfesionalId = profesionalId,
                Fecha = fecha,
                ServicioId = servicioId,
                HorariosDisponibles = horariosDisponibles
            });
        }

        private async Task CrearNotificacionConfirmacion(Cita cita)
        {
            var notificacion = new Notificacion
            {
                CitaId = cita.Id,
                Tipo = "Confirmacion",
                Destinatario = cita.Cliente.Email,
                Mensaje = $"Su cita ha sido confirmada para el {cita.FechaHoraInicio:dd/MM/yyyy} a las {cita.FechaHoraInicio:HH:mm}",
                Estado = "Pendiente"
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
        }

        private async Task CrearNotificacionCambioEstado(Cita cita)
        {
            var notificacion = new Notificacion
            {
                CitaId = cita.Id,
                Tipo = "CambioEstado",
                Destinatario = cita.Cliente.Email,
                Mensaje = $"El estado de su cita ha cambiado a: {cita.Estado}",
                Estado = "Pendiente"
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
        }
    }
}