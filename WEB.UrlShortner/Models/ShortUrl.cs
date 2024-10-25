using System.ComponentModel.DataAnnotations;

namespace WEB.UrlShortner.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }

        [Required]
        public string OriginalUrl { get; set; }

        public string ShortAlias { get; set; }

        public int ClickCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string QrCodeImagePath { get; set; }

        public string UserId { get; set; } // To associate with a user
    }
}
