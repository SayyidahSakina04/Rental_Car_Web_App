namespace RentalCarManagementSystem.Models.ViewModels
{
    public class PendingApprovalViewModel
    {
        public int BookingId { get; set; }
        public string CustomerEmail { get; set; }
        public string CarModel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
