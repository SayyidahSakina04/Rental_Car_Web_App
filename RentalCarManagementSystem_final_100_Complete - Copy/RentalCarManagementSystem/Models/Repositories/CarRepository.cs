using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RentalCarManagementSystem.Data;
using RentalCarManagementSystem.Models.Interfaces;
using RentalCarManagementSystem.Models.ViewModels;

namespace RentalCarManagementSystem.Models.Repositories
{
    public class CarRepository : ICarRepository
    {
        public List<Car> GetAllCarsForAdmin(ApplicationDbContext _context)
        {
            return _context.Cars.ToList();
        }

        public bool AddCar(Car car, ApplicationDbContext _context)
        {
            car.IsAvailable = true;
            _context.Cars.Add(car);
            _context.SaveChanges();
            return true;
        }
        public List<Car> GetAllCarsForCustomer(ApplicationDbContext _context)
        {
            return _context.Cars.Where(c => c.IsAvailable).ToList();
        }

        public Car GetCarById(ApplicationDbContext _context, int id) 
        {
            return _context.Cars.FirstOrDefault(c => c.Id == id);
        }
        List<ActiveRentalViewModel> ICarRepository.GetAllActiveRentals(ApplicationDbContext _context)
        {
            return (from booking in _context.Bookings
                    join car in _context.Cars on booking.CarId equals car.Id
                    join user in _context.Users on booking.UserId equals user.Id
                    where booking.Status == "Approved"
                    select new RentalCarManagementSystem.Models.ViewModels.ActiveRentalViewModel
                    {
                        BookingId = booking.BookingId,
                        CustomerEmail = user.Email,
                        CarModel = car.Model,
                        StartDate = booking.StartDate,
                        EndDate = booking.EndDate
                    }).ToList();
        }
        
    }
}
