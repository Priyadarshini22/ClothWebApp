using System.ComponentModel.DataAnnotations;
namespace ECommerceApp.Models
{
    // Represents the status master
    public class Status
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}