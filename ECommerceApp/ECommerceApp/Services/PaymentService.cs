using ECommerceApp.DTOs;
using ECommerceApp.DTOs.PaymentDTOs;
using ECommerceApp.Models;
using ECommerceApp.Repositories; // Import the new repository namespace
using ECommerceApp.Repository;
using Microsoft.Extensions.Configuration; // For IConfiguration
using Stripe; // For Stripe integration

namespace ECommerceApp.Services
{
    public class PaymentService
    {
        private readonly IPaymentRepository _paymentRepository; // Use the interface
        private readonly IEmailRepository _emailRepository;
        private readonly Stripe.ChargeService _stripeChargeService;

        public PaymentService(IPaymentRepository paymentRepository, IEmailRepository emailRepository, IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _emailRepository = emailRepository;

            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            _stripeChargeService = new Stripe.ChargeService();
        }

        public async Task<ApiResponse<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO paymentRequest)
        {
            try
            {
                return await _paymentRepository.ProcessPaymentAsync(paymentRequest);

            }
            catch (Exception ex)
            {
                // This catch block handles exceptions not caught by Stripe or other specific handlers.
                // It's good practice to log the full exception here.
                Console.WriteLine($"PaymentService.ProcessPaymentAsync error: {ex.Message}");
                return new ApiResponse<PaymentResponseDTO>(500, "An unexpected error occurred while processing the payment.");
            }
        }

        public async Task<ApiResponse<PaymentResponseDTO>> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                return new ApiResponse<PaymentResponseDTO>(404, "Payment not found.");
            }
            return new ApiResponse<PaymentResponseDTO>(200, MapPaymentToResponseDTO(payment));
        }

        public async Task<ApiResponse<PaymentResponseDTO>> GetPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
            {
                return new ApiResponse<PaymentResponseDTO>(404, "Payment not found for this order.");
            }
            return new ApiResponse<PaymentResponseDTO>(200, MapPaymentToResponseDTO(payment));
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdatePaymentStatusAsync(PaymentStatusUpdateDTO statusUpdate)
        {
            // Retrieve payment and its order to ensure business rules
            var payment = await _paymentRepository.GetPaymentByIdAsync(statusUpdate.PaymentId);
            if (payment == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Payment not found.");
            }

            var order = await _paymentRepository.GetOrderForPaymentAsync(payment.OrderId, 0); // CustomerId not strictly needed here
            if (order == null) // This should ideally not happen if payment.OrderId is valid
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Associated order not found for payment.");
            }

            payment.Status = statusUpdate.Status;

            if (statusUpdate.Status == PaymentStatus.Completed && !IsCashOnDelivery(payment.PaymentMethod))
            {
                payment.TransactionId = statusUpdate.TransactionId;
                // Only update order status to Processing if it's currently Pending
                if (order.OrderStatus == OrderStatus.Pending)
                {
                    await _paymentRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing);
                }
            }

            await _paymentRepository.UpdatePaymentAsync(payment);

            // Send Order Confirmation Email if Order Status is Processing
            if (order.OrderStatus == OrderStatus.Processing)
            {
                //var fullOrderDetails = await _paymentRepository.GetOrderDetailsForEmailAsync(order.Id);
                //if (fullOrderDetails != null)
                //{
                //    await SendOrderConfirmationEmailAsync(fullOrderDetails);
                //}
            }

            var confirmation = new ConfirmationResponseDTO
            {
                Message = $"Payment with ID {payment.Id} updated to status '{payment.Status}'."
            };
            return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> CompleteCODPaymentAsync(CODPaymentUpdateDTO codPaymentUpdateDTO)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(codPaymentUpdateDTO.PaymentId);
            if (payment == null || payment.OrderId != codPaymentUpdateDTO.OrderId)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "Payment not found for the specified order.");
            }

            var order = await _paymentRepository.GetOrderForPaymentAsync(payment.OrderId, 0); // CustomerId not strictly needed
            if (order == null)
            {
                return new ApiResponse<ConfirmationResponseDTO>(404, "No Order associated with this Payment.");
            }

            if (order.OrderStatus != OrderStatus.Shipped)
            {
                return new ApiResponse<ConfirmationResponseDTO>(400, $"Order cannot be marked as Delivered from {order.OrderStatus} State. It must be Shipped first.");
            }

            if (!IsCashOnDelivery(payment.PaymentMethod))
            {
                return new ApiResponse<ConfirmationResponseDTO>(409, "Payment method is not Cash on Delivery.");
            }

            payment.Status = PaymentStatus.Completed;
            order.OrderStatus = OrderStatus.Delivered; // Change order status to Delivered

            await _paymentRepository.UpdatePaymentAsync(payment);
            await _paymentRepository.UpdateOrderStatusAsync(order.Id, order.OrderStatus);

            var confirmation = new ConfirmationResponseDTO
            {
                Message = $"COD Payment for Order ID {order.Id} has been marked as 'Completed' and the order status updated to 'Delivered'."
            };
            return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
        }

        #region Helper Methods (now internal to service or private)

        private bool IsCashOnDelivery(string paymentMethod)
        {
            return paymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase) ||
                   paymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase);
        }

        private PaymentResponseDTO MapPaymentToResponseDTO(Payment payment)
        {
            return new PaymentResponseDTO
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status
            };
        }

        // Email sending method now takes the fully loaded Order object
        private async Task SendOrderConfirmationEmailAsync(Order order)
        {
            // No need to fetch order again, as it's passed in
            if (order == null || order.Customer == null || order.BillingAddress == null || order.ShippingAddress == null || order.OrderItems == null)
            {
                Console.WriteLine($"Missing data for email confirmation for Order ID: {order?.Id}. Cannot send email.");
                return;
            }

            var payment = order.Payment;

            string subject = $"Order Confirmation - {order.OrderNumber}";

            string emailBody = $@"
            <html>
              <head>
                <meta charset='UTF-8'>
              </head>
              <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
                <div style='max-width: 700px; margin: auto; background-color: #ffffff; padding: 20px; border: 1px solid #dddddd;'>
                  <div style='background-color: #007bff; padding: 15px; text-align: center; color: #ffffff;'>
                    <h2 style='margin: 0;'>Order Confirmation</h2>
                  </div>

                  <p style='margin: 20px 0 5px 0;'>Dear {order.Customer.FirstName} {order.Customer.LastName},</p>
                  <p style='margin: 5px 0 20px 0;'>Thank you for your order. Please find your invoice below.</p>
                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Order Number:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.OrderNumber}</td>
                    </tr>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Order Date:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.OrderDate:MMMM dd, yyyy}</td>
                    </tr>
                  </table>

                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Order Summary</h3>
                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Sub Total:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.TotalBaseAmount:C}</td>
                    </tr>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Discount:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>-{order.TotalDiscountAmount:C}</td>
                    </tr>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Shipping Cost:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.ShippingCost:C}</td>
                    </tr>
                    <tr style='font-weight: bold;'>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Total Amount:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.TotalAmount:C}</td>
                    </tr>
                  </table>

                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Order Items</h3>
                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr style='background-color: #28a745; color: #ffffff;'>
                      <th style='padding: 8px; border: 1px solid #dddddd;'>Product</th>
                      <th style='padding: 8px; border: 1px solid #dddddd;'>Quantity</th>
                      <th style='padding: 8px; border: 1px solid #dddddd;'>Unit Price</th>
                      <th style='padding: 8px; border: 1px solid #dddddd;'>Discount</th>
                      <th style='padding: 8px; border: 1px solid #dddddd;'>Total Price</th>
                    </tr>
                    {string.Join("", order.OrderItems.Select(item => $@"
                    <tr>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.Product.Name}</td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.Quantity}</td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.UnitPrice:C}</td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.Discount:C}</td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.TotalPrice:C}</td>
                    </tr>
                    "))}
                  </table>

                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Addresses</h3>
                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr>
                      <td style='width: 50%; vertical-align: top; padding: 8px; border: 1px solid #dddddd;'>
                        <strong>Billing Address</strong><br/>
                        {order.BillingAddress.AddressLine1}<br/>
                        {(string.IsNullOrWhiteSpace(order.BillingAddress.AddressLine2) ? "" : order.BillingAddress.AddressLine2 + "<br/>")}
                        {order.BillingAddress.City}, {order.BillingAddress.State} {order.BillingAddress.PostalCode}<br/>
                        {order.BillingAddress.Country}
                      </td>
                      <td style='width: 50%; vertical-align: top; padding: 8px; border: 1px solid #dddddd;'>
                        <strong>Shipping Address</strong><br/>
                        {order.ShippingAddress.AddressLine1}<br/>
                        {(string.IsNullOrWhiteSpace(order.ShippingAddress.AddressLine2) ? "" : order.ShippingAddress.AddressLine2 + "<br/>")}
                        {order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.PostalCode}<br/>
                        {order.ShippingAddress.Country}
                      </td>
                    </tr>
                  </table>

                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Payment Details</h3>
                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Payment Method:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.PaymentMethod : "N/A")}</td>
                    </tr>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Payment Date:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.PaymentDate.ToString("MMMM dd, yyyy HH:mm") : "N/A")}</td>
                    </tr>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Transaction ID:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.TransactionId : "N/A")}</td>
                    </tr>
                    <tr>
                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Status:</strong></td>
                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.Status.ToString() : "N/A")}</td>
                    </tr>
                  </table>

                  <p style='margin-top: 20px;'>If you have any questions, please contact our support team.</p>
                  <p>Best regards,<br/>Your E-Commerce Store Team</p>
                </div>
              </body>
            </html>";

            await _emailRepository.SendEmailAsync(order.Customer.Email, subject, emailBody, isBodyHtml: true);
        }

        #endregion
    }
}