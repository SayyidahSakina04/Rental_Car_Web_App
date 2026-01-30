using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RentalCarManagementSystem.Models
{
    public class MyUser : IdentityUser
    {
        [Required]
        public string role { get; set; }
        [Required]
        public string country { get; set; }
    }
}
