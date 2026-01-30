using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarManagementSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CarId { get; set; }
        public string UserId { get; set; }

        [MaxLength(24)]
        public string Status { get; set; } = "Pending";
        // pending, approved =  in-use, rejected, completed = returned
    }
}
