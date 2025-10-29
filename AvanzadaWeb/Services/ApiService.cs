using AvanzadaWeb.Models;
using System.Text;
using System.Text.Json;

namespace AvanzadaWeb.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<bool> PutAsync(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
        }

        private void AddUserIdHeader()
        {
            try
            {
                var userJson = _httpContextAccessor.HttpContext?.Session.GetString("User");
                if (!string.IsNullOrEmpty(userJson))
                {
                    var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);
                    if (sessionUser != null)
                    {
                        // Remover el header si ya existe para evitar duplicados
                        if (_httpClient.DefaultRequestHeaders.Contains("X-Usuario-ID"))
                        {
                            _httpClient.DefaultRequestHeaders.Remove("X-Usuario-ID");
                        }

                        // Agregar el header con el ID del usuario
                        _httpClient.DefaultRequestHeaders.Add("X-Usuario-ID", sessionUser.IDUsuario.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // Si falla la lectura de sesión, continuar sin el header
                // El middleware registrará como "Anónimo"
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            AddUserIdHeader();
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                AddUserIdHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en la solicitud POST: {ex.Message}", ex);
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            AddUserIdHeader();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            AddUserIdHeader();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            // Only check if the status code indicates success (2xx)
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            AddUserIdHeader();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}