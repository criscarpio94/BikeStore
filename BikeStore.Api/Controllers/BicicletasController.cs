using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeStore.Api.Data;
using BikeStore.Api.Models;


namespace BikeStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BicicletasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public BicicletasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bicicleta>>> GetBicicletas() =>
            await _context.Bicicleta.Include(b => b.Categoria).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Bicicleta>> GetBicicleta(int id)
        {
            var bicicleta = await _context.Bicicleta.Include(b => b.Categoria).FirstOrDefaultAsync(b => b.IdBicicleta == id);
            return bicicleta == null ? NotFound() : bicicleta;
        }

        [HttpPost]
        public async Task<ActionResult<Bicicleta>> PostBicicleta(Bicicleta bicicleta)
        {
            _context.Bicicleta.Add(bicicleta);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBicicleta), new { id = bicicleta.IdBicicleta }, bicicleta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBicicleta(int id, Bicicleta bicicleta)
        {
            if (id != bicicleta.IdBicicleta) return BadRequest();
            _context.Entry(bicicleta).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBicicleta(int id)
        {
            var bicicleta = await _context.Bicicleta.FindAsync(id);
            if (bicicleta == null) return NotFound();
            _context.Bicicleta.Remove(bicicleta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //Para buscar por nombre marca o categoria
        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<Bicicleta>>> Buscar(string? marca, int? idCategoria)
        {
            var query = _context.Bicicleta.Include(b => b.Categoria).AsQueryable();
            if (!string.IsNullOrEmpty(marca)) query = query.Where(b => b.Marca.Contains(marca));
            if (idCategoria.HasValue) query = query.Where(b => b.IdCategoria == idCategoria);
            return await query.ToListAsync();
        }

        //Consultar stock bajo
        [HttpGet("stock-bajo")]
        public async Task<ActionResult<IEnumerable<Bicicleta>>> GetStockBajo() =>
            await _context.Bicicleta.Include(b => b.Categoria).Where(b => b.Stock <= 5).ToListAsync();
    }
}
