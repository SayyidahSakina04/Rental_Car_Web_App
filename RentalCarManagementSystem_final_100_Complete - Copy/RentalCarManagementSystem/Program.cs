using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalCarManagementSystem.Data;
using RentalCarManagementSystem.Hubs;
using RentalCarManagementSystem.Models;
using RentalCarManagementSystem.Models.Interfaces;
using RentalCarManagementSystem.Models.Repositories;
using static RentalCarManagementSystem.Hubs.NotificationHub;

// Load .env file for local development
DotNetEnv.Env.Load();

// Enable legacy timestamp behavior for PostgreSQL (allows DateTime without UTC)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configure port for Render deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container.
// Use DATABASE_URL env var if available (for Render), otherwise use appsettings
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddDefaultIdentity<MyUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

//
builder.Services.AddRazorPages();

//
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("Role", "Admin"));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireClaim("Role", "Customer"));
});


var app = builder.Build();

// Seed database with roles, admin user, and sample cars
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only use HTTPS redirection in development (Render handles SSL)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();


//
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");


app.Run();
