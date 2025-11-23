using authService.Domain.Entities;

namespace authService.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetValidTokenAsync(string token);
    Task RevokeAsync(RefreshToken token);
}