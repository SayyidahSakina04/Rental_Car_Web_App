using Microsoft.AspNetCore.Identity;
using RentalCarManagementSystem.Models;

namespace RentalCarManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<MyUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            var adminEmail = "admin@rentalcar.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new MyUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    role = "Admin",
                    country = "Pakistan"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("Role", "Admin"));
                }
            }

            // Seed sample cars if none exist
            if (!context.Cars.Any())
            {
                var cars = new List<Car>
                {
                    new Car
                    {
                        Model = "Toyota Corolla 2024",
                        PricePerDay = 50.00m,
                        Description = "Reliable and fuel-efficient sedan, perfect for city driving and long trips.",
                        ImageURL = "https://images.unsplash.com/photo-1623869675781-80aa31012a5a?w=800",
                        IsAvailable = true
                    },
                    new Car
                    {
                        Model = "Honda Civic 2024",
                        PricePerDay = 55.00m,
                        Description = "Sporty sedan with excellent handling and modern features.",
                        ImageURL = "https://images.unsplash.com/photo-1606611013016-969c19ba27bb?w=800",
                        IsAvailable = true
                    },
                    new Car
                    {
                        Model = "BMW 3 Series",
                        PricePerDay = 120.00m,
                        Description = "Luxury sports sedan with premium interior and powerful performance.",
                        ImageURL = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800",
                        IsAvailable = true
                    },
                    new Car
                    {
                        Model = "Mercedes-Benz C-Class",
                        PricePerDay = 130.00m,
                        Description = "Elegant luxury sedan with cutting-edge technology and comfort.",
                        ImageURL = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800",
                        IsAvailable = true
                    },
                    new Car
                    {
                        Model = "Toyota Fortuner",
                        PricePerDay = 90.00m,
                        Description = "Powerful SUV ideal for family trips and off-road adventures.",
                        ImageURL = "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800",
                        IsAvailable = true
                    },
                    new Car
                    {
                        Model = "Honda City 2024",
                        PricePerDay = 45.00m,
                        Description = "Compact sedan with great mileage and comfortable interior.",
                        ImageURL = "https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800",
                        IsAvailable = true
                    }
                };

                context.Cars.AddRange(cars);
                await context.SaveChangesAsync();
            }
        }
    }
}
