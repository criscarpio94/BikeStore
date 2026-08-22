using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeStore.Api.Data;
using BikeStore.Api.Models;

namespace BikeStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas() =>
            await _context.Venta.Include(v => v.Cliente).Include(v => v.DetallesVenta).ThenInclude(d => d.Bicicleta).ToListAsync();


        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            var venta = await _context.Venta
                .Include(v => v.Cliente)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Bicicleta)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return venta;
        }

        [HttpPost]
        public async Task<ActionResult<Venta>> PostVenta(Venta venta)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //Validador de que la venta tenga productos
                if (venta.DetallesVenta == null || !venta.DetallesVenta.Any())
                {
                    return BadRequest("La venta debe contener al menos un producto");
                }

                //Fecha por defecto para la factura
                if (venta.Fecha == default)
                {
                    venta.Fecha = DateTime.Now;
                }

                //Para descontar el stock de forma automatica
                foreach (var detalle in venta.DetallesVenta)
                {
                    var bicicleta = await _context.Bicicleta.FindAsync(detalle.IdBicicleta);
                    if (bicicleta == null)
                    {
                        return BadRequest($"La bicicleta con Id: {detalle.IdBicicleta} no existe");
                    }

                    if (bicicleta.Stock < detalle.Cantidad)
                    {
                        return BadRequest($"Stock insuficientes para {bicicleta.Modelo}. Disponible: {bicicleta.Stock}, solicitado: {detalle.Cantidad}");
                    }

                    //Descontar cantidad en inventario
                    bicicleta.Stock -= detalle.Cantidad;
                    if (bicicleta.Stock == 0)
                    {
                        bicicleta.Estado = "Agotado";
                    }
                }

                //Agregar venta y detallas y guarda                
                _context.Venta.Add(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetVentas), new { id = venta.IdVenta }, venta);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var mensajeInterno = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"Error en servidor: {mensajeInterno}");
            }
        }
    }
}
