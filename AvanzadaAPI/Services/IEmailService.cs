namespace AvanzadaAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string nombre, string token);
    }
}