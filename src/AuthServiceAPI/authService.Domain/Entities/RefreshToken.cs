using System.ComponentModel.DataAnnotations;

namespace authService.Domain.Entities;

public class RefreshToken
{
    [Key]
    public int TokenId { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string Token { get; set; } = String.Empty;
    
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool Revoked { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}