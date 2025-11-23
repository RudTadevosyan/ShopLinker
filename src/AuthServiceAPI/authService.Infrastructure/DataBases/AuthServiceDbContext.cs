using authService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace authService.Infrastructure.DataBases
{
    public class AuthServiceDbContext : IdentityDbContext<User, IdentityRole<int>, int> //int => TKey 
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; } // RefreshToken Table
        
        public AuthServiceDbContext(DbContextOptions<AuthServiceDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // adding index for fast searches
            builder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();
        }
    }
}
