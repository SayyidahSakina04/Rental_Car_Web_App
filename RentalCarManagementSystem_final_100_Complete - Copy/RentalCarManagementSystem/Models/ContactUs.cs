using System.ComponentModel.DataAnnotations;

namespace RentalCarManagementSystem.Models
{
    public class ContactUs
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
