//using ECommerceApp.Data;
//using ECommerceApp.DTOs;
//using ECommerceApp.DTOs.PaymentDTOs;
//using ECommerceApp.Models;
//using Microsoft.EntityFrameworkCore;

//namespace ECommerceApp.Services
//{
//    public class PaymentService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly EmailService _emailService;

//        public PaymentService(ApplicationDbContext context, EmailService emailService)
//        {
//            _context = context;
//            _emailService = emailService;
//        }

//        public async Task<ApiResponse<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO paymentRequest)
//        {
//            // Use a transaction to guarantee atomic operations on Order and Payment
//            using (var transaction = await _context.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    // Retrieve the order along with any existing payment record
//                    var order = await _context.Orders
//                        .Include(o => o.Payment)
//                        .FirstOrDefaultAsync(o => o.Id == paymentRequest.OrderId && o.CustomerId == paymentRequest.CustomerId);

//                    if (order == null)
//                    {
//                        return new ApiResponse<PaymentResponseDTO>(404, "Order not found.");
//                    }

//                    if (Math.Round(paymentRequest.Amount, 2) != Math.Round(order.TotalAmount, 2))
//                    {
//                        return new ApiResponse<PaymentResponseDTO>(400, "Payment amount does not match the order total.");
//                    }

//                    Payment payment;

//                    // Check if an existing payment record is present
//                    if (order.Payment != null)
//                    {
//                        // Allow retry only if previous payment failed and order status is still Pending
//                        if (order.Payment.Status == PaymentStatus.Failed && order.OrderStatus == OrderStatus.Pending)
//                        {
//                            // Retry: update the existing payment record with new details
//                            payment = order.Payment;

//                            payment.PaymentMethod = paymentRequest.PaymentMethod;
//                            payment.Amount = paymentRequest.Amount;
//                            payment.PaymentDate = DateTime.UtcNow;
//                            payment.Status = PaymentStatus.Pending;
//                            payment.TransactionId = null; // Clear previous transaction id if any
//                            _context.Payments.Update(payment);
//                        }
//                        else
//                        {
//                            return new ApiResponse<PaymentResponseDTO>(400, "Order already has an associated payment.");
//                        }
//                    }
//                    else
//                    {
//                        // Create a new Payment record if none exists
//                        payment = new Payment
//                        {
//                            OrderId = paymentRequest.OrderId,
//                            PaymentMethod = paymentRequest.PaymentMethod,
//                            Amount = paymentRequest.Amount,
//                            PaymentDate = DateTime.UtcNow,
//                            Status = PaymentStatus.Pending
//                        };

//                        _context.Payments.Add(payment);
//                    }

//                    // For non-COD payments, simulate payment processing
//                    if (!IsCashOnDelivery(paymentRequest.PaymentMethod))
//                    {
//                        var simulatedStatus = await SimulatePaymentGateway();
//                        payment.Status = simulatedStatus;

//                        if (simulatedStatus == PaymentStatus.Completed)
//                        {
//                            // Update the Transaction Id on successful payment
//                            payment.TransactionId = GenerateTransactionId();

//                            // Update order status accordingly
//                            order.OrderStatus = OrderStatus.Processing;
//                        }
//                    }
//                    else
//                    {
//                        // For COD, mark the order status as Processing immediately
//                        order.OrderStatus = OrderStatus.Processing;
//                    }

//                    await _context.SaveChangesAsync();
//                    await transaction.CommitAsync();

//                    // Send Order Confirmation Email if Order Status is Processing
//                    // It means the user is either selected COD of the Payment is Sucessful 
//                    if (order.OrderStatus == OrderStatus.Processing)
//                    {
//                        await SendOrderConfirmationEmailAsync(paymentRequest.OrderId);
//                    }

//                    // Manual mapping to PaymentResponseDTO
//                    var paymentResponse = new PaymentResponseDTO
//                    {
//                        PaymentId = payment.Id,
//                        OrderId = payment.OrderId,
//                        PaymentMethod = payment.PaymentMethod,
//                        TransactionId = payment.TransactionId,
//                        Amount = payment.Amount,
//                        PaymentDate = payment.PaymentDate,
//                        Status = payment.Status
//                    };

//                    return new ApiResponse<PaymentResponseDTO>(200, paymentResponse);
//                }
//                catch (Exception)
//                {
//                    await transaction.RollbackAsync();
//                    return new ApiResponse<PaymentResponseDTO>(500, "An unexpected error occurred while processing the payment.");
//                }
//            }
//        }

//        public async Task<ApiResponse<PaymentResponseDTO>> GetPaymentByIdAsync(int paymentId)
//        {
//            try
//            {
//                var payment = await _context.Payments
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(p => p.Id == paymentId);

//                if (payment == null)
//                {
//                    return new ApiResponse<PaymentResponseDTO>(404, "Payment not found.");
//                }

//                var paymentResponse = new PaymentResponseDTO
//                {
//                    PaymentId = payment.Id,
//                    OrderId = payment.OrderId,
//                    PaymentMethod = payment.PaymentMethod,
//                    TransactionId = payment.TransactionId,
//                    Amount = payment.Amount,
//                    PaymentDate = payment.PaymentDate,
//                    Status = payment.Status
//                };

//                return new ApiResponse<PaymentResponseDTO>(200, paymentResponse);
//            }
//            catch (Exception)
//            {
//                return new ApiResponse<PaymentResponseDTO>(500, "An unexpected error occurred while retrieving the payment.");
//            }
//        }

//        public async Task<ApiResponse<PaymentResponseDTO>> GetPaymentByOrderIdAsync(int orderId)
//        {
//            try
//            {
//                var payment = await _context.Payments
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(p => p.OrderId == orderId);

//                if (payment == null)
//                {
//                    return new ApiResponse<PaymentResponseDTO>(404, "Payment not found for this order.");
//                }

//                var paymentResponse = new PaymentResponseDTO
//                {
//                    PaymentId = payment.Id,
//                    OrderId = payment.OrderId,
//                    PaymentMethod = payment.PaymentMethod,
//                    TransactionId = payment.TransactionId,
//                    Amount = payment.Amount,
//                    PaymentDate = payment.PaymentDate,
//                    Status = payment.Status
//                };

//                return new ApiResponse<PaymentResponseDTO>(200, paymentResponse);
//            }
//            catch (Exception)
//            {
//                return new ApiResponse<PaymentResponseDTO>(500, "An unexpected error occurred while retrieving the payment.");
//            }
//        }

//        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdatePaymentStatusAsync(PaymentStatusUpdateDTO statusUpdate)
//        {
//            try
//            {
//                var payment = await _context.Payments
//                    .Include(p => p.Order)
//                    .FirstOrDefaultAsync(p => p.Id == statusUpdate.PaymentId);

//                if (payment == null)
//                {
//                    return new ApiResponse<ConfirmationResponseDTO>(404, "Payment not found.");
//                }

//                payment.Status = statusUpdate.Status;

//                // Update order status if payment is now completed and the method is not COD
//                if (statusUpdate.Status == PaymentStatus.Completed && !IsCashOnDelivery(payment.PaymentMethod))
//                {
//                    payment.TransactionId = statusUpdate.TransactionId;
//                    payment.Order.OrderStatus = OrderStatus.Processing;
//                }

//                await _context.SaveChangesAsync();

//                // Send Order Confirmation Email if Order Status is Processing
//                if (payment.Order.OrderStatus == OrderStatus.Processing)
//                {
//                    await SendOrderConfirmationEmailAsync(payment.Order.Id);
//                }

//                var confirmation = new ConfirmationResponseDTO
//                {
//                    Message = $"Payment with ID {payment.Id} updated to status '{payment.Status}'."
//                };

//                return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
//            }
//            catch (Exception)
//            {
//                return new ApiResponse<ConfirmationResponseDTO>(500, "An unexpected error occurred while updating the payment status.");
//            }
//        }

//        public async Task<ApiResponse<ConfirmationResponseDTO>> CompleteCODPaymentAsync(CODPaymentUpdateDTO codPaymentUpdateDTO)
//        {
//            using (var transaction = await _context.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    var payment = await _context.Payments
//                    .Include(p => p.Order)
//                    .FirstOrDefaultAsync(p => p.Id == codPaymentUpdateDTO.PaymentId && p.OrderId == codPaymentUpdateDTO.OrderId);

//                    if (payment == null)
//                    {
//                        return new ApiResponse<ConfirmationResponseDTO>(404, "Payment not found.");
//                    }

//                    if (payment.Order == null)
//                    {
//                        return new ApiResponse<ConfirmationResponseDTO>(404, "No Order associated with this Payment.");
//                    }

//                    if (payment.Order.OrderStatus != OrderStatus.Shipped)
//                    {
//                        return new ApiResponse<ConfirmationResponseDTO>(400, $"Order cannot be marked as Delivered from {payment.Order.OrderStatus} State");
//                    }

//                    if (!IsCashOnDelivery(payment.PaymentMethod))
//                    {
//                        return new ApiResponse<ConfirmationResponseDTO>(409, "Payment method is not Cash on Delivery.");
//                    }

//                    payment.Status = PaymentStatus.Completed;
//                    payment.Order.OrderStatus = OrderStatus.Delivered;

//                    await _context.SaveChangesAsync();
//                    await transaction.CommitAsync();

//                    var confirmation = new ConfirmationResponseDTO
//                    {
//                        Message = $"COD Payment for Order ID {payment.Order.Id} has been marked as 'Completed' and the order status updated to 'Delivered'."
//                    };

//                    return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
//                }
//                catch (Exception)
//                {
//                    await transaction.RollbackAsync();
//                    return new ApiResponse<ConfirmationResponseDTO>(500, "An unexpected error occurred while completing the COD payment.");
//                }
//            }
//        }

//        #region Helper Methods

//        // Simulate a payment gateway response using Random.Shared for performance
//        private async Task<PaymentStatus> SimulatePaymentGateway()
//        {
//            //Simulate the PG
//            await Task.Delay(TimeSpan.FromMilliseconds(1));

//            int chance = Random.Shared.Next(1, 101); // 1 to 100

//            if (chance <= 60)
//                return PaymentStatus.Completed;
//            else if (chance <= 90)
//                return PaymentStatus.Pending;
//            else
//                return PaymentStatus.Failed;
//        }

//        // Generate a unique 12-character transaction ID
//        private string GenerateTransactionId()
//        {
//            return $"TXN-{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12)}";
//        }

//        // Determines if the provided payment method indicates Cash on Delivery
//        private bool IsCashOnDelivery(string paymentMethod)
//        {
//            return paymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase) ||
//                   paymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase);
//        }

//        // Fetches the complete order details (including discount, shipping cost, and summary),
//        // builds a professional HTML email body, and sends it to the customer.
//        public async Task SendOrderConfirmationEmailAsync(int orderId)
//        {
//            // Retrieve the order with its related customer, addresses, payment, and order items (with products)
//            var order = await _context.Orders
//                .AsNoTracking()
//                .Include(o => o.Customer)
//                .Include(o => o.BillingAddress)
//                .Include(o => o.ShippingAddress)
//                .Include(o => o.Payment)
//                .Include(o => o.OrderItems)
//                    .ThenInclude(oi => oi.Product)
//                .FirstOrDefaultAsync(o => o.Id == orderId);

//            if (order == null)
//            {
//                // Optionally log that the order was not found.
//                return;
//            }

//            var payment = order.Payment; // Payment details may be null if not available

//            // Prepare the email subject.
//            string subject = $"Order Confirmation - {order.OrderNumber}";

//            // Build the HTML email body using string interpolation.
//            string emailBody = $@"
//            <html>
//              <head>
//                <meta charset='UTF-8'>
//              </head>
//              <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;'>
//                <div style='max-width: 700px; margin: auto; background-color: #ffffff; padding: 20px; border: 1px solid #dddddd;'>
//                  <!-- Header -->
//                  <div style='background-color: #007bff; padding: 15px; text-align: center; color: #ffffff;'>
//                    <h2 style='margin: 0;'>Order Confirmation</h2>
//                  </div>
          
//                  <!-- Greeting and Order Details -->
//                  <p style='margin: 20px 0 5px 0;'>Dear {order.Customer.FirstName} {order.Customer.LastName},</p>
//                  <p style='margin: 5px 0 20px 0;'>Thank you for your order. Please find your invoice below.</p>
//                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Order Number:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.OrderNumber}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Order Date:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.OrderDate:MMMM dd, yyyy}</td>
//                    </tr>
//                  </table>
          
//                  <!-- Order Summary (placed before order items) -->
//                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Order Summary</h3>
//                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Sub Total:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.TotalBaseAmount:C}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Discount:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>-{order.TotalDiscountAmount:C}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Shipping Cost:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.ShippingCost:C}</td>
//                    </tr>
//                    <tr style='font-weight: bold;'>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Total Amount:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{order.TotalAmount:C}</td>
//                    </tr>
//                  </table>
          
//                  <!-- Order Items -->
//                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Order Items</h3>
//                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
//                    <tr style='background-color: #28a745; color: #ffffff;'>
//                      <th style='padding: 8px; border: 1px solid #dddddd;'>Product</th>
//                      <th style='padding: 8px; border: 1px solid #dddddd;'>Quantity</th>
//                      <th style='padding: 8px; border: 1px solid #dddddd;'>Unit Price</th>
//                      <th style='padding: 8px; border: 1px solid #dddddd;'>Discount</th>
//                      <th style='padding: 8px; border: 1px solid #dddddd;'>Total Price</th>
//                    </tr>
//                    {string.Join("", order.OrderItems.Select(item => $@"
//                    <tr>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.Product.Name}</td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.Quantity}</td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.UnitPrice:C}</td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.Discount:C}</td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{item.TotalPrice:C}</td>
//                    </tr>
//                    "))}
//                  </table>
          
//                  <!-- Addresses: Combined Billing and Shipping -->
//                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Addresses</h3>
//                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
//                    <tr>
//                      <td style='width: 50%; vertical-align: top; padding: 8px; border: 1px solid #dddddd;'>
//                        <strong>Billing Address</strong><br/>
//                        {order.BillingAddress.AddressLine1}<br/>
//                        {(string.IsNullOrWhiteSpace(order.BillingAddress.AddressLine2) ? "" : order.BillingAddress.AddressLine2 + "<br/>")}
//                        {order.BillingAddress.City}, {order.BillingAddress.State} {order.BillingAddress.PostalCode}<br/>
//                        {order.BillingAddress.Country}
//                      </td>
//                      <td style='width: 50%; vertical-align: top; padding: 8px; border: 1px solid #dddddd;'>
//                        <strong>Shipping Address</strong><br/>
//                        {order.ShippingAddress.AddressLine1}<br/>
//                        {(string.IsNullOrWhiteSpace(order.ShippingAddress.AddressLine2) ? "" : order.ShippingAddress.AddressLine2 + "<br/>")}
//                        {order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.PostalCode}<br/>
//                        {order.ShippingAddress.Country}
//                      </td>
//                    </tr>
//                  </table>
          
//                  <!-- Payment Details -->
//                  <h3 style='color: #007bff; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Payment Details</h3>
//                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Payment Method:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.PaymentMethod : "N/A")}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Payment Date:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.PaymentDate.ToString("MMMM dd, yyyy HH:mm") : "N/A")}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Transaction ID:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.TransactionId : "N/A")}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Status:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(payment != null ? payment.Status.ToString() : "N/A")}</td>
//                    </tr>
//                  </table>
          
//                  <p style='margin-top: 20px;'>If you have any questions, please contact our support team.</p>
//                  <p>Best regards,<br/>Your E-Commerce Store Team</p>
//                </div>
//              </body>
//            </html>";

//            // Send the email using the EmailService.
//            await _emailService.SendEmailAsync(order.Customer.Email, subject, emailBody, IsBodyHtml: true);
//        }

//        #endregion
//    }
//}