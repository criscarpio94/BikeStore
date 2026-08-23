using BikeStore.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreApi");
        }

        public async Task<IActionResult> Index(string? cedula, string? apellido)
        {
            var clientes = await _httpClient.GetFromJsonAsync<List<ClienteViewModel>>("api/Clientes");

            if (!string.IsNullOrEmpty(cedula))
            {
                clientes = clientes?.Where(c => c.Cedula.Contains(cedula)).ToList();
            }
            if (!string.IsNullOrEmpty(apellido))
            {
                clientes = clientes?.Where(c => c.Apellidos.Contains(apellido, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(clientes ?? new List<ClienteViewModel>());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClienteViewModel cliente)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Clientes", cliente);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", await ObtenerMensajeErrorApi(response, "No se pudo registrar el cliente."));
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _httpClient.GetFromJsonAsync<ClienteViewModel>($"api/Clientes/{id}");
            return cliente == null ? NotFound() : View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClienteViewModel cliente)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Clientes/{id}", cliente);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", await ObtenerMensajeErrorApi(response, "No se pudo actualizar el cliente."));
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Clientes/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Cliente eliminado correctamente.";
                TempData["TipoMensaje"] = "success";
            }
            else
            {
                TempData["Mensaje"] = await ObtenerMensajeErrorApi(response,
                    "No se pudo eliminar el cliente. Puede tener ventas registradas.");
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        private static async Task<string> ObtenerMensajeErrorApi(HttpResponseMessage response, string mensajePorDefecto)
        {
            var contenido = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(contenido) ? mensajePorDefecto : contenido.Trim('"');
        }
    }
}
