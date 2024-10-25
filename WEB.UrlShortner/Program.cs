using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB.UrlShortner.Data;
using WEB.UrlShortner.Models;
using WEB.UrlShortner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();


// Configure database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with custom settings
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.SignIn.RequireConfirmedAccount = false; // Optional: Disable email confirmation for testing
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Error handling based on environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Default MVC route configuration (Place this route first)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Custom route for URL aliases (Place this route after the default route)
app.MapControllerRoute(
    name: "shortUrl",
    pattern: "{alias}",
    defaults: new { controller = "Url", action = "RedirectToOriginal" });

// Map Razor Pages
app.MapRazorPages();

app.Run();
