using ECommerceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.ProductDTOs
{
    // DTO for returning product details.
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public int DiscountPercentage { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
        public string Image1 { get; set; }  // Required
        public string? Image2 { get; set; }  // Optional
        public IEnumerable<ProductSize>? ProductSizes { get; set; }

    }
}