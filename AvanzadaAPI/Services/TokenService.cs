using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Services
{
    public interface ITokenService
    {
        Task<string> GeneratePasswordResetTokenAsync(int userId);
        Task<PasswordResetToken?> ValidateTokenAsync(string token);
        Task<bool> MarkTokenAsUsedAsync(string token);
        Task CleanExpiredTokensAsync();
    }

    public class TokenService : ITokenService
    {
        private readonly AvanzadaContext _context;
        private readonly ILogger<TokenService> _logger;

        public TokenService(AvanzadaContext context, ILogger<TokenService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(int userId)
        {
            // Limpiar tokens expirados primero
            await CleanExpiredTokensAsync();

            // Generar token único
            var token = Guid.NewGuid().ToString() + "-" + DateTime.UtcNow.Ticks.ToString("x");

            var resetToken = new PasswordResetToken
            {
                IDUsuario = userId,
                Token = token,
                FechaExpiracion = DateTime.Now.AddHours(12),
                Utilizado = false,
                FechaCreacion = DateTime.Now
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<PasswordResetToken?> ValidateTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(rt => rt.Usuario)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.Utilizado);

            if (resetToken == null)
            {
                _logger.LogWarning("Token no encontrado o ya utilizado: {Token}", token);
                return null;
            }

            if (resetToken.FechaExpiracion < DateTime.UtcNow)
            {
                _logger.LogWarning("Token expirado: {Token}", token);
                return null;
            }

            return resetToken;
        }

        public async Task<bool> MarkTokenAsUsedAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (resetToken != null)
            {
                resetToken.Utilizado = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = _context.PasswordResetTokens
                .Where(rt => rt.FechaExpiracion < DateTime.UtcNow || rt.Utilizado);

            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tokens expirados eliminados: {Count}", await expiredTokens.CountAsync());
        }
    }
}