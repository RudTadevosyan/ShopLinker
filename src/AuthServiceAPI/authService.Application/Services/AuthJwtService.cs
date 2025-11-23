using authService.Domain.CustomExceptions;
using authService.Domain.Entities;
using authService.Domain.Interfaces;
using AuthService.Shared.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace authService.Application.Services
{
    public class AuthJwtService : IAuthJwtService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthJwtService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration config, IMapper mapper, IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _mapper = mapper;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<JwtDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = _mapper.Map<User>(registerDto);

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                throw new NotFoundException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // add the role for the user (by def Client)
            await _userManager.AddToRoleAsync(user, "Client");
            
            return await GenerateTokensAsync(user);
        }

        public async Task<JwtDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email)
                ?? throw new NotFoundException($"User with email {loginDto.Email} not found");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                throw new DomainException($"Invalid password.");

            return await GenerateTokensAsync(user);
        }

        public async Task DeleteAsync(DeleteDto deleteUserDto)
        {
            var user = await _userManager.FindByEmailAsync(deleteUserDto.Email)
                ?? throw new NotFoundException($"User with email {deleteUserDto.Email} not found");

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, deleteUserDto.Password, false);
            if (!passwordCheck.Succeeded)
                throw new DomainException($"Invalid password.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new Exception($"Failed to delete user.");
        }
        
        public async Task<JwtDto> RefreshAsync(string refreshToken)
        {
            var stored = await _refreshTokenRepository.GetValidTokenAsync(refreshToken);
            if (stored == null)
                throw new Exception("Invalid or expired refresh token.");

            // revoke old token
            await _refreshTokenRepository.RevokeAsync(stored);

            // generate new tokens
            return await GenerateTokensAsync(stored.User);
        }
        
        public async Task ChangePasswordAsync(UpdateDto updateDto)
        {
            var user = await _userManager.FindByEmailAsync(updateDto.Email)
                       ?? throw new NotFoundException($"User with email {updateDto.Email} not found");
            
            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, updateDto.Password, false);
            if(!passwordCheck.Succeeded)
                throw new DomainException($"Invalid password.");
            
            var result = await _userManager.ChangePasswordAsync(user, updateDto.Password, updateDto.NewPassword);
            if(!result.Succeeded)
                throw new DomainException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Private Helpers for generating and refreshing JWT tokens
        private async Task<JwtDto> GenerateTokensAsync(User user)
        {
            var accessToken = await GenerateJwt(user);

            var refreshToken = new RefreshToken
            {
                // creating new unique refresh token 
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                UserId = user.Id,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new JwtDto(accessToken.Jwt, refreshToken.Token);
        }

        private async Task<JwtDto> GenerateJwt(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtDto(new JwtSecurityTokenHandler().WriteToken(token), string.Empty);
        }

    }
}