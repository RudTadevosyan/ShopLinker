using AuthService.Shared.DTOs;

namespace authService.Domain.Interfaces
{
    public interface IAuthJwtService
    {
        Task<JwtDto> LoginAsync(LoginDto loginDto);
        Task<JwtDto> RegisterAsync(RegisterDto registerDto);
        Task<JwtDto> RefreshAsync(string refreshToken);
        Task DeleteAsync(DeleteDto deleteDto);
        Task ChangePasswordAsync(UpdateDto updateDto);
    }
}
