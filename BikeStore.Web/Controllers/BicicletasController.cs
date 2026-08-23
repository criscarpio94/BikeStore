using Microsoft.AspNetCore.Mvc;
using BikeStore.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace BikeStore.Web.Controllers
{
    public class BicicletasController : Controller
    {
        private readonly HttpClient _httpClient;
        public BicicletasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BikeStoreApi");
        }

        
        public async Task<IActionResult> Index(string? marca, int? idCategoria, bool stockBajo = false)
        {
            HttpResponseMessage response;
            if (!string.IsNullOrEmpty(marca) || idCategoria.HasValue)
            {
                var parametros = new List<string>();
                if (!string.IsNullOrEmpty(marca))
                    parametros.Add($"marca={Uri.EscapeDataString(marca)}");
                if (idCategoria.HasValue)
                    parametros.Add($"idCategoria={idCategoria.Value}");

                response = await _httpClient.GetAsync($"api/Bicicletas/buscar?{string.Join("&", parametros)}");
            }
            else
            {
                response = await _httpClient.GetAsync("api/Bicicletas");
            }

            var categorias = await _httpClient.GetFromJsonAsync<List<CategoriaViewModel>>("api/Categorias") ?? new();
            ViewBag.Categorias = categorias;
            ViewBag.IdCategoriaSeleccionada = idCategoria?.ToString() ?? "";

            if (response.IsSuccessStatusCode)
            {
                var bicicletas = await response.Content.ReadFromJsonAsync<List<BicicletaViewModel>>() ?? new List<BicicletaViewModel>();

                if (stockBajo)
                    bicicletas = bicicletas.Where(b => b.Stock <= 5).ToList();

                return View(bicicletas);
            }

            return View(new List<BicicletaViewModel>());
        }

        //Get para crear
        public async Task<IActionResult> Create()
        {
            var model = new BicicletaViewModel();
            await CargarCategorias(model);
            return View(model);
        }

        //POST

        [HttpPost]
        public async Task<IActionResult> Create(BicicletaViewModel bicicleta)
        {
            var response = await _httpClient.PostAsJsonAsync("api/bicicletas", bicicleta);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", await ObtenerMensajeErrorApi(response, "No se pudo registrar la bicicleta."));
            await CargarCategorias(bicicleta);
            return View(bicicleta);
        }

        //Get Edicion de Bicicletas
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/Bicicletas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var bicicleta = await response.Content.ReadFromJsonAsync<BicicletaViewModel>();
                return View(bicicleta);
            }
            return NotFound();
        }

        //Guardar los cambios al editar
        [HttpPost]
        public async Task<IActionResult> Edit(int id, BicicletaViewModel bicicleta)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Bicicletas/{id}", bicicleta);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(bicicleta);
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Bicicletas/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Bicicleta eliminada correctamente.";
                TempData["TipoMensaje"] = "success";
            }
            else
            {
                TempData["Mensaje"] = await ObtenerMensajeErrorApi(response,
                    "No se pudo eliminar la bicicleta. Puede estar asociada a una venta.");
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategorias(BicicletaViewModel model)
        {
            var categorias = await _httpClient.GetFromJsonAsync<List<CategoriaViewModel>>("api/Categorias") ?? new();
            model.CategoriasList = categorias
                .Where(c => c.Activo)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCategoria.ToString(),
                    Text = c.Nombre
                })
                .ToList();
        }

        private static async Task<string> ObtenerMensajeErrorApi(HttpResponseMessage response, string mensajePorDefecto)
        {
            var contenido = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(contenido) ? mensajePorDefecto : contenido.Trim('"');
        }
    }
}
