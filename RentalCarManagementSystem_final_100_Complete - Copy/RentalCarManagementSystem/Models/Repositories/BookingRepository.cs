using Microsoft.EntityFrameworkCore;
using RentalCarManagementSystem.Data;
using RentalCarManagementSystem.Models.Interfaces;
using RentalCarManagementSystem.Models.ViewModels;

namespace RentalCarManagementSystem.Models.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        public List<BookingHistoryViewModel> GetBookingHistoryForAdmin(ApplicationDbContext _context)
        {
            return _context.Bookings
                    .Join(_context.Users,
                          booking => booking.UserId,
                          user => user.Id,
                          (booking, user) => new
                          {
                              booking.BookingId,
                              user.Email,
                              booking.CarId,
                              booking.StartDate,
                              booking.EndDate,
                              booking.Status
                          })
                    .Join(_context.Cars,
                          b => b.CarId,
                          car => car.Id,
                          (b, car) => new RentalCarManagementSystem.Models.ViewModels.BookingHistoryViewModel
                          {
                              BookingId = b.BookingId,
                              CustomerEmail = b.Email,
                              CarModel = car.Model,
                              StartDate = b.StartDate,
                              EndDate = b.EndDate,
                              Status = b.Status
                          })
                    .ToList();
        }

        public List<PendingApprovalViewModel> GetPendingApprovalForAdmin(ApplicationDbContext _context)
        {
            return (from booking in _context.Bookings
                 join car in _context.Cars on booking.CarId equals car.Id
                 join user in _context.Users on booking.UserId equals user.Id
                 where booking.Status == "Pending"
                 select new RentalCarManagementSystem.Models.ViewModels.PendingApprovalViewModel
                 {
                     BookingId = booking.BookingId,
                     CustomerEmail = user.Email,
                     CarModel = car.Model,
                     StartDate = booking.StartDate,
                     EndDate = booking.EndDate
                 }).ToList();
        }

        public void MakeABooking(ApplicationDbContext _context, string userId, int carId, DateTime startDate, DateTime endDate)
        {
            var booking = new Booking
            {
                CarId = carId,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();
        }

        public void ReturnCar(ApplicationDbContext _context, int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null || booking.Status != "Approved")
                return;

            booking.Status = "Returned";

            var car = _context.Cars.FirstOrDefault(c => c.Id == booking.CarId);
            if (car != null)
                car.IsAvailable = true;

            _context.SaveChanges();
        }

        void IBookingRepository.CancelBooking(ApplicationDbContext _context, int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.Status == "Pending");
            if (booking == null)
                return;

            booking.Status = "Cancelled";
            _context.SaveChanges();
        }

        List<Booking> IBookingRepository.GetBookingsOfUser(ApplicationDbContext _context, string userId)
        {
            return _context.Bookings.Where(b => b.UserId == userId)
                .ToList();
        }
        void IBookingRepository.ApproveBooking(ApplicationDbContext _context, int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking != null)
            {
                booking.Status = "Approved";

                var car = _context.Cars.FirstOrDefault(c => c.Id == booking.CarId);
                if (car != null)
                {
                    car.IsAvailable = false;
                }

                _context.SaveChanges();
            }
        }
        void IBookingRepository.RejectBooking(ApplicationDbContext _context, int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking != null)
            {
                booking.Status = "Rejected";
                _context.SaveChanges();
            }
        }
    }
}
