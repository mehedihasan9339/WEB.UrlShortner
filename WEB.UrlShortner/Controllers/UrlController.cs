using Microsoft.AspNetCore.Mvc;
using WEB.UrlShortner.Models;
using WEB.UrlShortner.Data;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using WEB.UrlShortner.Services;

public class UrlController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UrlController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Url/Index
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var shortUrls = await _context.ShortUrls
            .Where(s => s.UserId == userId)
            .ToListAsync();

        ViewBag.ChartData = shortUrls.Select(url => new {
            Date = url.CreatedAt.ToString("yyyy-MM-dd"),
            Clicks = url.ClickCount
        });

        return View(shortUrls);
    }

    // GET: Url/Create
    public IActionResult Create() => View();

    // POST: Url/Create
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

        // Send email notification
        try
        {
            var recipientEmail = User?.Identity.Name; // Assuming the user email is available through User
            var subject = "Your URL Has Been Shortened!";
            var baseUrl = $"https://localhost:44329/"; // Adjust as necessary
            var shortUrlLink = $"{baseUrl}{generatedAlias}";


            // Specify the path to the QR code image for attachment
            var qrCodeDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot").ToString();
            var qrCodePath = qrCodeDirectory + shortUrl.QrCodeImagePath; // Ensure this path is correct and accessible



            // Create the email body with a professional design
            var body = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f7f9fc;
                        color: #333;
                        margin: 0;
                        padding: 20px;
                    }}
                    .container {{
                        max-width: 600px;
                        background-color: #ffffff;
                        border-radius: 8px;
                        padding: 30px;
                        box-shadow: 0 4px 15px rgba(0,0,0,0.1);
                        margin: auto;
                    }}
                    h2 {{
                        color: #007BFF;
                        text-align: center;
                    }}
                    p {{
                        line-height: 1.6;
                    }}
                    .footer {{
                        margin-top: 30px;
                        font-size: 0.9em;
                        color: #777;
                        text-align: center;
                    }}
                    .qr-code {{
                        display: block;
                        margin: 20px auto;
                        max-width: 200px;
                    }}
                    .button {{
                        display: inline-block;
                        margin-top: 20px;
                        padding: 10px 15px;
                        background-color: #007BFF;
                        color: white;
                        text-decoration: none;
                        border-radius: 5px;
                        text-align: center;
                    }}
                    .button:hover {{
                        background-color: #0056b3;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Hello!</h2>
                    <p>Your original URL: <strong>{originalUrl}</strong> has been shortened successfully!</p>
                    <p>Here is your shortened URL: <a href='{shortUrlLink}'>{shortUrlLink}</a></p>
                    <img src='{qrCodePath}' alt='QR Code' class='qr-code'/>
                    <p>Thank you for using our URL shortening service!</p>
                    <div class='footer'>
                        <p>Best regards,</p>
                        <p>Your URL Shortener Team</p>
                    </div>
                </div>
            </body>
            </html>";

            
            // Send the email with the QR code attached
            await _emailService.SendEmailAsync(recipientEmail, subject, body, qrCodePath);
        }
        catch (Exception ex)
        {
            // Log the error or handle as needed
            ModelState.AddModelError("", "URL shortened successfully, but email notification failed.");
        }

        return RedirectToAction("Index");
    }

    // POST: Url/Delete/{alias}
    [HttpPost]
    public async Task<IActionResult> Delete(string alias)
    {
        var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(s => s.ShortAlias == alias);

        if (shortUrl == null) return NotFound();

        // Delete the URL from the database
        _context.ShortUrls.Remove(shortUrl);
        await _context.SaveChangesAsync();

        // Send email notification about deletion
        try
        {
            var recipientEmail = User?.Identity.Name; // Assuming the user email is available through User
            var subject = "Your Short URL Has Been Deleted";
            var body = $@"
                <html>
                <body>
                    <h2>Notification of Deletion</h2>
                    <p>Your short URL with the alias <strong>{alias}</strong> has been successfully deleted.</p>
                    <p>If this was a mistake, please contact support.</p>
                    <p>Thank you for using our URL shortening service!</p>
                </body>
                </html>";

            // Send the email
            await _emailService.SendEmailAsync(recipientEmail, subject, body);
        }
        catch (Exception ex)
        {
            // Log the error or handle as needed
            ModelState.AddModelError("", "Short URL deleted successfully, but email notification failed.");
        }

        return RedirectToAction("Index");
    }

    // Redirect to Original URL
    public async Task<IActionResult> RedirectToOriginal(string alias)
    {
        var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(s => s.ShortAlias == alias);

        if (shortUrl == null) return NotFound();

        shortUrl.ClickCount++;
        await _context.SaveChangesAsync();

        return Redirect(shortUrl.OriginalUrl);
    }

    // Generates a Random Alias
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

    // GET: Url/CheckAliasAvailability
    [HttpGet]
    public async Task<JsonResult> CheckAliasAvailability(string alias)
    {
        var isAvailable = !await _context.ShortUrls.AnyAsync(s => s.ShortAlias == alias);
        return Json(new { available = isAvailable });
    }
}
