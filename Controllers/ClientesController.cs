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
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClientesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetClientes()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return Ok(_mapper.Map<List<ClienteDTO>>(clientes));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return _mapper.Map<ClienteDTO>(cliente);
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> PostCliente(CrearClienteDTO crearClienteDTO)
        {
            var cliente = _mapper.Map<Cliente>(crearClienteDTO);
            
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, 
                _mapper.Map<ClienteDTO>(cliente));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteDTO clienteDTO)
        {
            if (id != clienteDTO.Id)
            {
                return BadRequest();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            _mapper.Map(clienteDTO, cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}