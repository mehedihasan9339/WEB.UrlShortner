using System.ComponentModel.DataAnnotations;

namespace WEB.UrlShortner.Models
{
    public class UrlEntry
    {
        [Key]
        public int Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortUrl { get; set; }
        public string Alias { get; set; }
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
