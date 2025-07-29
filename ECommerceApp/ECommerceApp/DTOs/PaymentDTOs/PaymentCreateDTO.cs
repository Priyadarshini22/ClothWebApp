using ECommerceApp.DTOs.AddressesDTOs;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.PaymentDTOs
{
    public class PaymentCreateDTO
    {

        public int CustomerId { get; set; }

        public int OrderId { get; set; }

        public int BillingId { get; set; }

        public string? PaymentMethod { get; set; } // e.g., "CreditCard", "DebitCard", "PayPal", "COD"

        public decimal Amount { get; set; }
        public string PaymentId { get; set; } // Add this


    }

}
