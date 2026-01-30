namespace RentalCarManagementSystem.Models.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public List<Booking> CurrentBookings { get; set; }
        public List<Booking> BookingHistory { get; set; }
    }
}
