using BikeStore.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeStore.Web.Controllers
{
    public class VentasController : Controller
    {
        private readonly HttpClient _httpClient;

        public VentasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreApi");
        }

        public async Task<IActionResult> Index(string? cedula, int? clienteId)
        {
            var response = await _httpClient.GetAsync("api/Ventas");
            if (response.IsSuccessStatusCode)
            {
                var ventasApi = await response.Content.ReadFromJsonAsync<List<VentaApiResponseDto>>() ?? new List<VentaApiResponseDto>();

                var ventas = ventasApi.Select(MapToViewModel).ToList();

                // Filtrar por ID de Cliente
                if (clienteId.HasValue)
                {
                    ventas = ventas.Where(v => v.IdCliente == clienteId.Value).ToList();
                }

                // Filtrar por Cédula si se ingresó en un buscador
                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    ventas = ventas.Where(v => v.CedulaCliente.Contains(cedula.Trim())).ToList();
                }

                return View(ventas);
            }

            return View(new List<VentaViewModel>());
        }

        // --- DTOs auxiliares completos ---
        public class VentaApiResponseDto
        {
            public int IdVenta { get; set; }
            public int IdCliente { get; set; }
            public DateTime Fecha { get; set; }
            public decimal Total { get; set; }
            public ClienteViewModel? Cliente { get; set; }
            public List<DetalleVentaDto>? DetallesVenta { get; set; }
        }

        public class DetalleVentaDto
        {
            public int IdBicicleta { get; set; }
            public int Cantidad { get; set; }
            public decimal Precio { get; set; }
            public decimal Subtotal { get; set; }
            public BicicletaViewModel? Bicicleta { get; set; }
        }

        // --- Método Details Ajustado ---
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"api/Ventas/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var dto = await response.Content.ReadFromJsonAsync<VentaApiResponseDto>();
            if (dto == null)
                return RedirectToAction(nameof(Index));

            return View(MapToViewModel(dto));
        }

        [HttpPost]
        public async Task<IActionResult> BuscarCliente(VentaCreateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CedulaBusqueda))
            {
                ModelState.AddModelError("", "Ingrese una cédula para buscar.");
                await CargarDesplegables(model);
                return View("Create", model);
            }

            var response = await _httpClient.GetAsync($"api/Clientes/buscar?cedula={Uri.EscapeDataString(model.CedulaBusqueda.Trim())}");
            if (response.IsSuccessStatusCode)
            {
                var clientes = await response.Content.ReadFromJsonAsync<List<ClienteViewModel>>() ?? new();
                var cliente = clientes.FirstOrDefault();
                if (cliente != null)
                {
                    model.IdCliente = cliente.IdCliente;
                    model.NombreCliente = $"{cliente.Nombres} {cliente.Apellidos}";
                }
                else
                {
                    ModelState.AddModelError("", "No se encontró un cliente con esa cédula.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Error al buscar el cliente.");
            }

            await CargarDesplegables(model);
            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProducto(VentaCreateViewModel model)
        {
            if (model.IdBicicletaSeleccionada > 0)
            {
                var response = await _httpClient.GetAsync($"api/Bicicletas/{model.IdBicicletaSeleccionada}");
                if (response.IsSuccessStatusCode)
                {
                    var bici = await response.Content.ReadFromJsonAsync<BicicletaViewModel>();
                    if (bici != null)
                    {
                        var existente = model.Detalles.FirstOrDefault(d => d.IdBicicleta == bici.IdBicicleta);
                        if (existente != null)
                        {
                            var nuevaCantidad = existente.Cantidad + model.CantidadSeleccionada;
                            if (nuevaCantidad > bici.Stock)
                            {
                                ModelState.AddModelError("",
                                    $"Stock insuficiente para {bici.Modelo}. Ya tiene {existente.Cantidad} en la venta. Disponible: {bici.Stock}");
                            }
                            else
                            {
                                existente.Cantidad = nuevaCantidad;
                            }
                        }
                        else if (model.CantidadSeleccionada > bici.Stock)
                        {
                            ModelState.AddModelError("", $"Stock insuficiente para {bici.Modelo}. Disponible: {bici.Stock}");
                        }
                        else
                        {
                            model.Detalles.Add(new DetalleItemViewModel
                            {
                                IdBicicleta = bici.IdBicicleta,
                                NombreBicicleta = $"{bici.Modelo} ({bici.Marca})",
                                Precio = bici.Precio,
                                Cantidad = model.CantidadSeleccionada,
                                StockDisponible = bici.Stock
                            });
                        }
                    }
                }
            }
            await CargarDesplegables(model);
            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarProducto(VentaCreateViewModel model, int index)
        {
            if (index >= 0 && index < model.Detalles.Count)
            {
                model.Detalles.RemoveAt(index);
            }
            await CargarDesplegables(model);
            return View("Create", model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new VentaCreateViewModel();
            await CargarDesplegables(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VentaCreateViewModel model)
        {
            if (model.IdCliente <= 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente.");
                await CargarDesplegables(model);
                return View(model);
            }

            if (!model.Detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto a la venta.");
                await CargarDesplegables(model);
                return View(model);
            }

            var payload = new
            {
                IdCliente = model.IdCliente,
                Fecha = DateTime.Now,
                Total = model.Total,
                DetallesVenta = model.Detalles.Select(d => new
                {
                    IdBicicleta = d.IdBicicleta,
                    Cantidad = d.Cantidad,
                    Precio = d.Precio
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync("api/Ventas", payload);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error en API: {errorMsg}");
            await CargarDesplegables(model);
            return View(model);
        }

        private static VentaViewModel MapToViewModel(VentaApiResponseDto v)
        {
            var subtotal = v.DetallesVenta != null && v.DetallesVenta.Any()
                ? v.DetallesVenta.Sum(d => d.Subtotal)
                : Math.Round(v.Total / 1.15m, 2);

            var iva = Math.Round(v.Total - subtotal, 2);

            return new VentaViewModel
            {
                IdVenta = v.IdVenta,
                IdCliente = v.IdCliente,
                NombreCliente = v.Cliente != null ? $"{v.Cliente.Nombres} {v.Cliente.Apellidos}" : "Sin Cliente",
                CedulaCliente = v.Cliente?.Cedula ?? "",
                Fecha = v.Fecha,
                Subtotal = subtotal,
                Iva = iva,
                Total = v.Total,
                Detalles = v.DetallesVenta?.Select(d => new DetalleItemViewModel
                {
                    IdBicicleta = d.IdBicicleta,
                    NombreBicicleta = d.Bicicleta != null ? $"{d.Bicicleta.Modelo} ({d.Bicicleta.Marca})" : $"Bicicleta #{d.IdBicicleta}",
                    Cantidad = d.Cantidad,
                    Precio = d.Precio
                }).ToList() ?? new List<DetalleItemViewModel>()
            };
        }

        private async Task CargarDesplegables(VentaCreateViewModel model)
        {
            var respClientes = await _httpClient.GetAsync("api/Clientes");
            if (respClientes.IsSuccessStatusCode)
            {
                var lista = await respClientes.Content.ReadFromJsonAsync<List<ClienteViewModel>>() ?? new();
                model.ClientesList = lista.Select(c => new SelectListItem
                {
                    Value = c.IdCliente.ToString(),
                    Text = $"{c.Cedula} - {c.Nombres} {c.Apellidos}"
                }).ToList();
            }

            var respBicis = await _httpClient.GetAsync("api/Bicicletas");
            if (respBicis.IsSuccessStatusCode)
            {
                var lista = await respBicis.Content.ReadFromJsonAsync<List<BicicletaViewModel>>() ?? new();
                model.BicicletasList = lista.Where(b => b.Stock > 0).Select(b => new SelectListItem
                {
                    Value = b.IdBicicleta.ToString(),
                    Text = $"{b.Modelo} - {b.Marca} (Stock: {b.Stock})"
                }).ToList();
            }
        }
    }
}