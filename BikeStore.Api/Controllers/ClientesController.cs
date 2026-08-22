using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeStore.Api.Data;
using BikeStore.Api.Models;

namespace BikeStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes() =>
            await _context.Cliente.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            return cliente == null ? NotFound() : cliente;
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCliente), new { id = cliente.IdCliente }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.IdCliente) return BadRequest();
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null) return NotFound();
            _context.Cliente.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //Para buscar clientes por cedula o apellido
        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<Cliente>>> Buscar(string? cedula, string? apellido)
        {
            var query = _context.Cliente.AsQueryable();
            if (!string.IsNullOrEmpty(cedula)) query = query.Where(c => c.Cedula == cedula);
            if (!string.IsNullOrEmpty(apellido)) query = query.Where(c => c.Apellidos.Contains(apellido));
            return await query.ToListAsync();
        }
    }
}
