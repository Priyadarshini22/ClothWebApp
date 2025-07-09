using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models.QueryModel
{
    public class ProductQueryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int DiscountPercentage { get; set; }
        public bool IsAvailable { get; set; }
        public byte[]? Image1 { get; set; } 
        public byte[]? Image2 { get; set; }
        public int CategoryId { get; set; }
        public int SizeId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; }
    }
}
