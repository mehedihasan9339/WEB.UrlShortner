using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Diagnostics;
using System.Drawing.Imaging;
using WEB.UrlShortner.Data;
using WEB.UrlShortner.Models;
using WEB.UrlShortner.Services;

namespace WEB.UrlShortner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Url/Index");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string originalUrl, string alias = "")
        {
            if (string.IsNullOrEmpty(originalUrl))
            {
                ModelState.AddModelError("", "Original URL is required.");
                return View();
            }

            // Generate a random alias if no custom alias is provided
            string generatedAlias = GenerateRandomAlias();

            // If a custom alias is provided, check if it already exists
            if (!string.IsNullOrEmpty(alias))
            {
                if (await _context.ShortUrls.AnyAsync(s => s.ShortAlias == alias))
                {
                    ModelState.AddModelError("", "Custom alias already exists. Please choose another.");
                    return View();
                }
                generatedAlias = alias;
            }

            var userId = _userManager.GetUserId(User);

            // Create the ShortUrl object
            var shortUrl = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortAlias = generatedAlias,
                UserId = userId,
                ClickCount = 0,
                QrCodeImagePath = GenerateQrCodeImage(generatedAlias) // Generate QR code
            };

            // Save the short URL to the database
            _context.ShortUrls.Add(shortUrl);
            await _context.SaveChangesAsync();



            return RedirectToAction("Details", new { id = shortUrl.Id });
        }

        // GET: Url/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var shortUrl = await _context.ShortUrls.FindAsync(id);
            if (shortUrl == null)
            {
                return NotFound();
            }
            return View(shortUrl);
        }
        public async Task<IActionResult> RedirectToOriginal(string alias)
        {
            var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(s => s.ShortAlias == alias);

            if (shortUrl == null) return NotFound();

            shortUrl.ClickCount++;
            await _context.SaveChangesAsync();

            return Redirect(shortUrl.OriginalUrl);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private string GenerateRandomAlias(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        // Generate QR Code and Save as Image
        private string GenerateQrCodeImage(string alias)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(alias, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            using var qrImage = qrCode.GetGraphic(20);
            string path = Path.Combine("wwwroot", "qrcodes", $"{alias}.png");

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            qrImage.Save(path, ImageFormat.Png);

            return $"/qrcodes/{alias}.png";
        }
    }
}
