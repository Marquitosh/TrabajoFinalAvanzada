//// AvanzadaWeb/Controllers/LogsController.cs
//using AvanzadaWeb.Services;
//using AvanzadaWeb.ViewModels;
//using Microsoft.AspNetCore.Mvc;

//namespace AvanzadaWeb.Controllers
//{
//    public class LogsController : Controller
//    {
//        private readonly IApiService _apiService;

//        public LogsController(IApiService apiService)
//        {
//            _apiService = apiService;
//        }

//        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta, string? nivel, string? usuario)
//        {
//            try
//            {
//                // Construir query string para filtros
//                var queryParams = new List<string>();

//                if (fechaDesde.HasValue)
//                    queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");

//                if (fechaHasta.HasValue)
//                    queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");

//                if (!string.IsNullOrEmpty(nivel))
//                    queryParams.Add($"nivel={nivel}");

//                if (!string.IsNullOrEmpty(usuario))
//                    queryParams.Add($"usuario={usuario}");

//                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

//                // Llamar a la API
//                var logs = await _apiService.GetAsync<List<LogViewModel>>($"logs{queryString}");

//                // CREAR EL MODELO WRAPPER - ESTA ES LA PARTE CLAVE
//                var model = new LogsIndexViewModel
//                {
//                    Logs = logs ?? new List<LogViewModel>(),
//                    FechaDesde = fechaDesde,
//                    FechaHasta = fechaHasta,
//                    Nivel = nivel,
//                    Usuario = usuario
//                };

//                // DEVOLVER LogsIndexViewModel, NO List<LogViewModel>
//                return View(model);
//            }
//            catch (Exception ex)
//            {
//                TempData["Message"] = $"Error al cargar los logs: {ex.Message}";

//                // También aquí devolver LogsIndexViewModel vacío
//                return View(new LogsIndexViewModel());
//            }
//        }
//    }
//}