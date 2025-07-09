namespace ECommerceApp.Models
{
    public class ProductSize
    {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string? Size { get; set; }
            public int Quantity { get; set; }
            public Product? Product { get; set; }
    }
}
    