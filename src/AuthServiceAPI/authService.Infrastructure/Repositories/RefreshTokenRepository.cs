using authService.Domain.Entities;
using authService.Domain.Interfaces;
using authService.Infrastructure.DataBases;
using Microsoft.EntityFrameworkCore;

namespace authService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthServiceDbContext _context;

    public RefreshTokenRepository(AuthServiceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetValidTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && !r.Revoked && r.ExpiresAtUtc > DateTime.UtcNow);
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.Revoked = true;
        await _context.SaveChangesAsync();
    }
}