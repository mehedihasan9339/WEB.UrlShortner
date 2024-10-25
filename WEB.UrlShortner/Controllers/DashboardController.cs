using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB.UrlShortner.Data;
using WEB.UrlShortner.Models;
using Microsoft.AspNetCore.Identity;

namespace WEB.UrlShortner.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var userUrls = await _context.ShortUrls
                .Where(url => url.UserId == userId)
                .ToListAsync();

            return View(userUrls);
        }
    }
}
