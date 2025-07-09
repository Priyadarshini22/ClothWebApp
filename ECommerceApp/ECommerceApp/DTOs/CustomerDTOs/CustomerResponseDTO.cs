namespace ECommerceApp.DTOs.CustomerDTOs
{
    // DTO for returning customer details.
    public class CustomerResponseDTO
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}