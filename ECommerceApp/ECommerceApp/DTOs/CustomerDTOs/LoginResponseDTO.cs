namespace ECommerceApp.DTOs.CustomerDTOs
{
    public class LoginResponseDTO
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }
    }
}