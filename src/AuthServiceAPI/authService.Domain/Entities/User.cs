using Microsoft.AspNetCore.Identity;

namespace authService.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;

    }
}
