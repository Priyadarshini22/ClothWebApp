//using Microsoft.EntityFrameworkCore;
//using ECommerceApp.Data;
//using ECommerceApp.Models;
//using ECommerceApp.DTOs.RefundDTOs;
//using ECommerceApp.DTOs;

//namespace ECommerceApp.Services
//{
//    public class RefundService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly EmailService _emailService;

//        public RefundService(ApplicationDbContext context, EmailService emailService)
//        {
//            _context = context;
//            _emailService = emailService;
//        }

//        // Retrieves all eligible cancellations for refund initiation.
//        // (i.e. approved cancellations with no associated refund record)
//        public async Task<ApiResponse<List<PendingRefundResponseDTO>>> GetEligibleRefundsAsync()
//        {
//            var eligible = await _context.Cancellations
//                .Include(c => c.Order)
//                    .ThenInclude(o => o.Payment)
//                .Where(c => c.Status == CancellationStatus.Approved && c.Refund == null
//                            && c.Order.Payment.PaymentMethod.ToLower() != "COD")
//                .Select(c => new PendingRefundResponseDTO
//                {
//                    CancellationId = c.Id,
//                    OrderId = c.OrderId,
//                    OrderAmount = c.OrderAmount,
//                    CancellationCharge = c.CancellationCharges ?? 0.00m,
//                    ComputedRefundAmount = c.OrderAmount - (c.CancellationCharges ?? 0.00m),
//                    CancellationRemarks = c.Remarks
//                }).ToListAsync();

//            return new ApiResponse<List<PendingRefundResponseDTO>>(200, eligible);
//        }

//        // The ProcessRefundAsync method in the service will handles only those cancellations that are approved
//        // and have no entry in the Refunds table.
//        public async Task<ApiResponse<RefundResponseDTO>> ProcessRefundAsync(RefundRequestDTO refundRequest)
//        {
//            // Retrieve the cancellation with related Order, Payment, and Customer details.
//            var cancellation = await _context.Cancellations
//                .Include(c => c.Order)
//                    .ThenInclude(o => o.Payment)
//                .Include(c => c.Order)
//                    .ThenInclude(o => o.Customer)
//                .FirstOrDefaultAsync(c => c.Id == refundRequest.CancellationId);

//            if (cancellation == null)
//                return new ApiResponse<RefundResponseDTO>(404, "Cancellation request not found.");

//            if (cancellation.Status != CancellationStatus.Approved)
//                return new ApiResponse<RefundResponseDTO>(400, "Only approved cancellations are eligible for refunds.");

//            // Only proceed if no refund record exists.
//            var existingRefund = await _context.Refunds
//                .FirstOrDefaultAsync(r => r.CancellationId == refundRequest.CancellationId);

//            if (existingRefund != null)
//                return new ApiResponse<RefundResponseDTO>(400, "Refund for this cancellation request has already been initiated.");

//            // Validate that a Payment record exists.
//            var payment = cancellation.Order.Payment;
//            if (payment == null || payment.PaymentMethod.ToLower() == "COD")
//                return new ApiResponse<RefundResponseDTO>(400, "No payment associated with the order.");

//            // Compute the refund amount.
//            decimal computedRefundAmount = cancellation.OrderAmount - (cancellation.CancellationCharges ?? 0.00m);
//            if (computedRefundAmount <= 0)
//                return new ApiResponse<RefundResponseDTO>(400, "Computed refund amount is not valid.");

//            // Create a new refund record.
//            var refund = new Refund
//            {
//                CancellationId = refundRequest.CancellationId,
//                PaymentId = payment.Id,
//                Amount = computedRefundAmount,
//                RefundMethod = refundRequest.RefundMethod.ToString(),
//                RefundReason = refundRequest.RefundReason,
//                Status = RefundStatus.Pending,
//                InitiatedAt = DateTime.UtcNow,
//                ProcessedBy = refundRequest.ProcessedBy
//            };

//            _context.Refunds.Add(refund);
//            await _context.SaveChangesAsync();

//            // Immediately try processing via the simulated payment gateway.
//            var gatewayResponse = await ProcessRefundPaymentAsync(refund);
//            if (gatewayResponse.IsSuccess)
//            {
//                refund.Status = RefundStatus.Completed;
//                refund.TransactionId = gatewayResponse.TransactionId;
//                refund.CompletedAt = DateTime.UtcNow;

//                payment.Status = PaymentStatus.Refunded;
//                _context.Payments.Update(payment);

//                // Send email notification.
//                if (cancellation.Order.Customer != null && !string.IsNullOrEmpty(cancellation.Order.Customer.Email))
//                {
//                    await _emailService.SendEmailAsync(
//                        cancellation.Order.Customer.Email,
//                         $"Your Refund Has Been Processed Successfully, Order #{cancellation.Order.OrderNumber}",
//                        GenerateRefundSuccessEmailBody(refund, cancellation.Order.OrderNumber, cancellation),
//                        IsBodyHtml: true);
//                }
//            }
//            else
//            {
//                refund.Status = RefundStatus.Failed;
//            }

//            _context.Refunds.Update(refund);
//            await _context.SaveChangesAsync();

//            return new ApiResponse<RefundResponseDTO>(200, MapRefundToDTO(refund));
//        }

//        // The UpdateRefundStatus method now allows an admin to manually process refunds that are either pending or failed.
//        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateRefundStatusAsync(RefundStatusUpdateDTO statusUpdate)
//        {
//            var refund = await _context.Refunds
//                .Include(r => r.Cancellation)
//                    .ThenInclude(c => c.Order)
//                        .ThenInclude(o => o.Customer)
//                .Include(r => r.Payment)
//                .FirstOrDefaultAsync(r => r.Id == statusUpdate.RefundId);

//            if (refund == null)
//                return new ApiResponse<ConfirmationResponseDTO>(404, "Refund not found.");

//            // Allow manual update only if refund is Pending or Failed.
//            if (refund.Status != RefundStatus.Pending && refund.Status != RefundStatus.Failed)
//                return new ApiResponse<ConfirmationResponseDTO>(400, "Only pending or failed refunds can be updated.");

//            // In a manual update, we reprocess the refund.
//            refund.RefundMethod = statusUpdate.RefundMethod.ToString();
//            refund.Status = RefundStatus.Completed;
//            refund.TransactionId = statusUpdate.TransactionId;
//            refund.CompletedAt = DateTime.UtcNow;
//            refund.ProcessedBy = statusUpdate.ProcessedBy;
//            refund.RefundReason = statusUpdate.RefundReason;

//            //Also mark the Payment Status as Refunded
//            refund.Payment.Status = PaymentStatus.Refunded;
//            _context.Payments.Update(refund.Payment);
//            _context.Refunds.Update(refund);
//            await _context.SaveChangesAsync();

//            // Send email notification.
//            if (refund.Cancellation?.Order?.Customer != null && !string.IsNullOrEmpty(refund.Cancellation.Order.Customer.Email))
//            {
//                await _emailService.SendEmailAsync(
//                    refund.Cancellation.Order.Customer.Email,
//                    $"Your Refund Has Been Processed Successfully, Order #{refund.Cancellation.Order.OrderNumber}",
//                    GenerateRefundSuccessEmailBody(refund, refund.Cancellation.Order.OrderNumber, refund.Cancellation),
//                    IsBodyHtml: true);
//            }

//            var confirmation = new ConfirmationResponseDTO
//            {
//                Message = $"Refund with ID {refund.Id} has been updated to {refund.Status}."
//            };

//            return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
//        }

//        // Retrieves a refund by its ID.
//        public async Task<ApiResponse<RefundResponseDTO>> GetRefundByIdAsync(int id)
//        {
//            var refund = await _context.Refunds
//                .Include(r => r.Cancellation)
//                    .ThenInclude(c => c.Order)
//                        .ThenInclude(o => o.Payment)
//                .FirstOrDefaultAsync(r => r.Id == id);

//            if (refund == null)
//                return new ApiResponse<RefundResponseDTO>(404, "Refund not found.");

//            return new ApiResponse<RefundResponseDTO>(200, MapRefundToDTO(refund));
//        }

//        // Retrieves all refunds.
//        public async Task<ApiResponse<List<RefundResponseDTO>>> GetAllRefundsAsync()
//        {
//            var refunds = await _context.Refunds
//                .Include(r => r.Cancellation)
//                    .ThenInclude(c => c.Order)
//                        .ThenInclude(o => o.Payment)
//                .ToListAsync();
//            var refundList = refunds.Select(r => MapRefundToDTO(r)).ToList();
//            return new ApiResponse<List<RefundResponseDTO>>(200, refundList);
//        }

//        // Private Helper Methods
//        private RefundResponseDTO MapRefundToDTO(Refund refund)
//        {
//            return new RefundResponseDTO
//            {
//                Id = refund.Id,
//                CancellationId = refund.CancellationId,
//                PaymentId = refund.PaymentId,
//                Amount = refund.Amount,
//                RefundMethod = Enum.Parse<RefundMethod>(refund.RefundMethod),
//                RefundReason = refund.RefundReason,
//                Status = refund.Status,
//                InitiatedAt = refund.InitiatedAt,
//                CompletedAt = refund.CompletedAt,
//                TransactionId = refund.TransactionId
//            };
//        }

//        // Simulates calling a payment gateway to process the refund.
//        // In production, replace this with actual integration code.
//        public async Task<PaymentGatewayRefundResponseDTO> ProcessRefundPaymentAsync(Refund refund)
//        {
//            // Simulate a network delay of 1 second.
//            await Task.Delay(TimeSpan.FromSeconds(1));

//            // Create a Random instance. (In production, consider reusing a static instance.)
//            var random = new Random();
//            double chance = random.NextDouble(); // Generates a double between 0.0 and 1.0.

//            if (chance < 0.70) // 70% chance for Completed.
//            {
//                return new PaymentGatewayRefundResponseDTO
//                {
//                    IsSuccess = true,
//                    Status = RefundStatus.Completed,
//                    TransactionId = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
//                };
//            }
//            else if (chance < 0.90) // Next 20% chance for Failed.
//            {
//                return new PaymentGatewayRefundResponseDTO
//                {
//                    IsSuccess = false,
//                    Status = RefundStatus.Failed
//                };
//            }
//            else // Remaining 10% chance for Pending.
//            {
//                return new PaymentGatewayRefundResponseDTO
//                {
//                    IsSuccess = false,
//                    Status = RefundStatus.Pending
//                };
//            }
//        }

//        // Generates an HTML email body (with inline CSS) to notify the customer.
//        public string GenerateRefundSuccessEmailBody(Refund refund, string orderNumber, Cancellation cancellation)
//        {
//            // Format CompletedAt if available; otherwise show "N/A".

//            // Define the IST time zone.
//            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

//            // Convert CompletedAt from UTC to IST, if available.
//            string completedAtStr = refund.CompletedAt.HasValue
//                ? TimeZoneInfo.ConvertTimeFromUtc(refund.CompletedAt.Value, istZone).ToString("dd MMM yyyy HH:mm:ss")
//                : "N/A";

//            return $@"
//            <html>
//            <body style='font-family: Arial, sans-serif; margin: 0; padding: 0;'>
//                <div style='background-color: #f4f4f4; padding: 20px;'>
//                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border: 1px solid #ddd;'>
//                        <div style='padding: 20px; text-align: center; background-color: #2E86C1; color: #ffffff;'>
//                            <h2>Your Refund is Complete</h2>
//                        </div>
//                        <div style='padding: 20px;'>
//                            <p>Dear Customer,</p>
//                            <p>Your refund has been processed successfully. Below are the details:</p>
//                            <table style='width: 100%; border-collapse: collapse;'>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Order Number</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>{orderNumber}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Refund Transaction ID</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>{refund.TransactionId}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Order Amount</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>₹{cancellation.OrderAmount}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Cancellation Charges</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>₹{cancellation.CancellationCharges ?? 0.00m}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Cancellation Reason</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>{cancellation.Reason}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Refunded Method</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>{refund.RefundMethod}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>Refunded Amount</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>₹{refund.Amount}</td>
//                                </tr>
//                                <tr>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>CompletedAt At</td>
//                                    <td style='border: 1px solid #ddd; padding: 8px;'>{completedAtStr}</td>
//                                </tr>
//                            </table>
//                            <p>Thank you for shopping with us.</p>
//                            <p>Best regards,<br/>The ECommerce Team</p>
//                        </div>
//                    </div>
//                </div>
//            </body>
//            </html>";
//        }
//    }
//}