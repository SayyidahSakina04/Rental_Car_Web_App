using RentalCarManagementSystem.Data;
using RentalCarManagementSystem.Models.ViewModels;

namespace RentalCarManagementSystem.Models.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> GetBookingsOfUser(ApplicationDbContext _context, string userId);
        //List<object> GetBookingHistory(ApplicationDbContext _context, List<Booking> bookings);
        void MakeABooking(ApplicationDbContext _context, string userId, int carId, DateTime startDate, DateTime endDate);
        void CancelBooking(ApplicationDbContext _context, int bookingId);
        void ReturnCar(ApplicationDbContext _context, int bookingId);
        List<BookingHistoryViewModel> GetBookingHistoryForAdmin(ApplicationDbContext _context);
        List<PendingApprovalViewModel> GetPendingApprovalForAdmin(ApplicationDbContext _context);
        void ApproveBooking(ApplicationDbContext _context, int id);
        void RejectBooking(ApplicationDbContext _context, int id);
    }
}
