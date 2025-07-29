using ECommerceApp.DTOs;
using ECommerceApp.DTOs.PaymentDTOs;
using ECommerceApp.Models;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Configuration;

namespace ECommerceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly AddressService _addressService;
        private readonly IConfiguration _configuration;
        public PaymentsController(PaymentService paymentService, IConfiguration configuration, AddressService addressService)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _addressService = addressService;
        }

        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentCreateDTO createPaymentRequest)
        {
            StripeConfiguration.ApiKey = _configuration.GetValue<string>("Stripe:SecretKey");
            var address = await _addressService.GetAddressByIdAsync(createPaymentRequest.BillingId);
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(createPaymentRequest.Amount * 100), 
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Description = $"Payment for Order",
                Shipping = new ChargeShippingOptions
                {
                    Name = createPaymentRequest.CustomerId.ToString(),
                    Address = new AddressOptions
                    {
                        Line1 = address.Data.AddressLine1,
                        Line2 = address.Data.AddressLine2,
                        City = address.Data.City,
                        State = address.Data.State,
                        PostalCode = address.Data.PostalCode,
                        Country = address.Data.Country
                    }
                }
                
            };
            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }

        [HttpPost("ProcessPayment")]
        public async Task<ActionResult<ApiResponse<PaymentResponseDTO>>> ProcessPayment([FromBody] PaymentRequestDTO paymentRequest)
        {
            var response = await _paymentService.ProcessPaymentAsync(paymentRequest);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves payment details by Payment ID.
        [HttpGet("GetPaymentById/{paymentId}")]
        public async Task<ActionResult<ApiResponse<PaymentResponseDTO>>> GetPaymentById(int paymentId)
        {
            var response = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves payment details associated with a specific order.
        [HttpGet("GetPaymentByOrderId/{orderId}")]
        public async Task<ActionResult<ApiResponse<PaymentResponseDTO>>> GetPaymentByOrderId(int orderId)
        {
            var response = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Updates the status of an existing payment.
        [HttpPut("UpdatePaymentStatus")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> UpdatePaymentStatus([FromBody] PaymentStatusUpdateDTO statusUpdate)
        {
            var response = await _paymentService.UpdatePaymentStatusAsync(statusUpdate);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Completes a Cash on Delivery (COD) payment.
        [HttpPost("CompleteCODPayment")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> CompleteCODPayment([FromBody] CODPaymentUpdateDTO codPaymentUpdateDTO)
        {
            var response = await _paymentService.CompleteCODPaymentAsync(codPaymentUpdateDTO);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }
    }
}