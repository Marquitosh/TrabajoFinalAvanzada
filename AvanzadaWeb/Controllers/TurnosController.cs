using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    public class TurnosController : Controller
    {
        private readonly IApiService _apiService;

        public TurnosController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var turnos = await _apiService.GetAsync<List<TurnoViewModel>>("turnos");
            return View(turnos);
        }

        public IActionResult Create() 
        {
            return RedirectToAction("RequestService", "Usuarios");
        }

        [HttpPost]
        public async Task<IActionResult> Create(TurnoViewModel turno)
        {
            if (ModelState.IsValid)
            {
                await _apiService.PostAsync<TurnoViewModel>("turnos", turno);
                return RedirectToAction(nameof(Index));
            }
            return View(turno);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var turno = await _apiService.GetAsync<TurnoViewModel>($"turnos/{id}");
            return View(turno);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TurnoViewModel turno)
        {
            if (ModelState.IsValid)
            {
                await _apiService.PutAsync<TurnoViewModel>($"turnos/{id}", turno);
                return RedirectToAction(nameof(Index));
            }
            return View(turno);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteAsync($"turnos/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}