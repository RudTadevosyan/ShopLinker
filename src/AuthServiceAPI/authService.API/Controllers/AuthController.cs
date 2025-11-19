using authService.Domain.Interfaces;
using AuthService.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace authService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthJwtService _authService;

        public AuthController(IAuthJwtService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var tokens = await _authService.RegisterAsync(registerDto);

            // Set refresh token cookie
            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                // must be HTTPS
                SameSite = SameSiteMode.None, // for cross-origin via Ocelot
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var tokens = await _authService.LoginAsync(loginDto);

            // Set refresh token cookie
            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteDto deleteUserDto)
        {
            await _authService.DeleteAsync(deleteUserDto);
            return Ok(new { message = "User deleted successfully." });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // Read refresh token from HttpOnly cookie
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token missing." });

            var tokens = await _authService.RefreshAsync(refreshToken);

            // Set new refresh token cookie
            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }
        
        [HttpPut("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] UpdateDto updateDto)
        {
            await _authService.ChangePasswordAsync(updateDto);
            return Ok();
        }
    }
}
