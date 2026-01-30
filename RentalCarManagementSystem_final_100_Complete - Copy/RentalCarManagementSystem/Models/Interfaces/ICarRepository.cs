using Microsoft.EntityFrameworkCore;
using RentalCarManagementSystem.Data;
using RentalCarManagementSystem.Models.ViewModels;

namespace RentalCarManagementSystem.Models.Interfaces
{
    public interface ICarRepository
    {
        // to show objects
        public List<Car> GetAllCarsForAdmin(ApplicationDbContext _context);

        // to add objects
        public bool AddCar(Car car, ApplicationDbContext _context);

        // will add the remaining later on if needed 
        public List<Car> GetAllCarsForCustomer(ApplicationDbContext _context);

        public Car GetCarById(ApplicationDbContext _context, int id);

        List<ActiveRentalViewModel> GetAllActiveRentals(ApplicationDbContext _context);
    }
}
