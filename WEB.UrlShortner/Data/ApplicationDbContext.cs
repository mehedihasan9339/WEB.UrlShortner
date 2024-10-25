using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEB.UrlShortner.Models;

namespace WEB.UrlShortner.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UrlEntry> UrlEntries { get; set; }
        public DbSet<ShortUrl> ShortUrls { get; set; }

    }
}
