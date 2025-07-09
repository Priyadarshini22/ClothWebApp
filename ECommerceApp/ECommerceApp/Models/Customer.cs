using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ECommerceApp.Models
{
    // Represents a customer in the e-commerce system
    [Index(nameof(Email), Name = "IX_Email_Unique", IsUnique = true)]
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 50 characters.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last Name must be between 2 and 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "PhoneNumber is required.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "DateOfBirth is required.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Address>? Addresses { get; set; }
        public ICollection<Order>? Orders { get; set; }

        // Navigation property: A user can have many carts but only 1 active cart
        public ICollection<Cart>? Carts { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }
    }
}