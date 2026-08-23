using BikeStore.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Web.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly HttpClient _httpClient;

        public CategoriasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreApi");
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _httpClient.GetFromJsonAsync<List<CategoriaViewModel>>("api/Categorias");
            return View(categorias ?? new List<CategoriaViewModel>());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoriaViewModel categoria)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Categorias", categoria);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", await ObtenerMensajeErrorApi(response, "No se pudo crear la categoría."));
            return View(categoria);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _httpClient.GetFromJsonAsync<CategoriaViewModel>($"api/Categorias/{id}");
            return categoria == null ? NotFound() : View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoriaViewModel categoria)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Categorias/{id}", categoria);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", await ObtenerMensajeErrorApi(response, "No se pudo actualizar la categoría."));
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Categorias/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Categoría eliminada correctamente.";
                TempData["TipoMensaje"] = "success";
            }
            else
            {
                TempData["Mensaje"] = await ObtenerMensajeErrorApi(response,
                    "No se pudo eliminar la categoría. Puede tener bicicletas asociadas.");
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
