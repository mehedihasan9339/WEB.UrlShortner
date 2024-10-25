using Microsoft.AspNetCore.Identity;

namespace WEB.UrlShortner.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<UrlEntry> UrlEntries { get; set; }
    }
}
