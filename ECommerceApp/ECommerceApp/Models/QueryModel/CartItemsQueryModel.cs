using ECommerceApp.DTOs.ShoppingCartDTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models.QueryModel
{
    public class CartItemsQueryModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public bool IsCheckedOut { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public byte[]? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
