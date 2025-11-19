namespace AuthService.Shared.DTOs;

public record UpdateDto(string Email, string Password, string NewPassword);