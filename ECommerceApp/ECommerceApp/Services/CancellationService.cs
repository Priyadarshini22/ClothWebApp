//using Microsoft.EntityFrameworkCore;
//using ECommerceApp.Data;
//using ECommerceApp.DTOs.CancellationDTOs;
//using ECommerceApp.Models;
//using ECommerceApp.DTOs;

//namespace ECommerceApp.Services
//{
//    public class CancellationService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly EmailService _emailService;

//        public CancellationService(ApplicationDbContext context, EmailService emailService)
//        {
//            _context = context;
//            _emailService = emailService;
//        }

//        // Handles a cancellation request from a customer.
//        public async Task<ApiResponse<CancellationResponseDTO>> RequestCancellationAsync(CancellationRequestDTO cancellationRequest)
//        {
//            try
//            {
//                // Validate order existence with its items and product details (read-only)
//                var order = await _context.Orders
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(o => o.Id == cancellationRequest.OrderId &&
//                                         o.CustomerId == cancellationRequest.CustomerId);

//                if (order == null)
//                {
//                    return new ApiResponse<CancellationResponseDTO>(404, "Order not found.");
//                }

//                // Check if order is eligible for cancellation (only Processing)
//                if (order.OrderStatus != OrderStatus.Processing)
//                {
//                    return new ApiResponse<CancellationResponseDTO>(400, "Order is not eligible for cancellation.");
//                }

//                // Check if a cancellation request for the order already exists
//                var existingCancellation = await _context.Cancellations
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(c => c.OrderId == cancellationRequest.OrderId);

//                if (existingCancellation != null)
//                {
//                    return new ApiResponse<CancellationResponseDTO>(400, "A cancellation request for this order already exists.");
//                }

//                // Create the new cancellation record
//                var cancellation = new Cancellation
//                {
//                    OrderId = cancellationRequest.OrderId,
//                    Reason = cancellationRequest.Reason,
//                    Status = CancellationStatus.Pending,
//                    RequestedAt = DateTime.UtcNow,
//                    OrderAmount = order.TotalAmount,
//                    CancellationCharges = 0.00m, // default zero; admin may update later if needed.
//                };

//                _context.Cancellations.Add(cancellation);
//                await _context.SaveChangesAsync();

//                // Mapping from Cancellation to CancellationResponseDTO
//                var cancellationResponse = new CancellationResponseDTO
//                {
//                    Id = cancellation.Id,
//                    OrderId = cancellation.OrderId,
//                    Reason = cancellation.Reason,
//                    OrderAmount = order.TotalAmount,
//                    Status = cancellation.Status,
//                    RequestedAt = cancellation.RequestedAt,
//                    CancellationCharges = cancellation.CancellationCharges
//                };

//                return new ApiResponse<CancellationResponseDTO>(200, cancellationResponse);
//            }
//            catch (Exception ex)
//            {
//                // Log exception as needed
//                return new ApiResponse<CancellationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
//            }
//        }

//        // Retrieves a cancellation request by its ID.
//        public async Task<ApiResponse<CancellationResponseDTO>> GetCancellationByIdAsync(int id)
//        {
//            try
//            {
//                var cancellation = await _context.Cancellations
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(c => c.Id == id);

//                if (cancellation == null)
//                {
//                    return new ApiResponse<CancellationResponseDTO>(404, "Cancellation request not found.");
//                }

//                var cancellationResponse = new CancellationResponseDTO
//                {
//                    Id = cancellation.Id,
//                    OrderId = cancellation.OrderId,
//                    Reason = cancellation.Reason, //Provided by Client
//                    Status = cancellation.Status,
//                    RequestedAt = cancellation.RequestedAt,
//                    ProcessedAt = cancellation.ProcessedAt,
//                    ProcessedBy = cancellation.ProcessedBy,
//                    Remarks = cancellation.Remarks, //Provided by Admin
//                    OrderAmount = cancellation.OrderAmount,
//                    CancellationCharges = cancellation.CancellationCharges
//                };

//                return new ApiResponse<CancellationResponseDTO>(200, cancellationResponse);
//            }
//            catch (Exception ex)
//            {
//                return new ApiResponse<CancellationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
//            }
//        }

//        // Updates the status of a cancellation request (approval/rejection) by an administrator.
//        // Also handles order status update and stock restoration if approved.
//        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateCancellationStatusAsync(CancellationStatusUpdateDTO statusUpdate)
//        {
//            // Begin a transaction to ensure atomic operations
//            using (var transaction = await _context.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    var cancellation = await _context.Cancellations
//                        .Include(c => c.Order)
//                            .ThenInclude(cust => cust.Customer)
//                        .FirstOrDefaultAsync(c => c.Id == statusUpdate.CancellationId);

//                    if (cancellation == null)
//                    {
//                        return new ApiResponse<ConfirmationResponseDTO>(404, "Cancellation request not found.");
//                    }

//                    if (cancellation.Status != CancellationStatus.Pending)
//                    {
//                        return new ApiResponse<ConfirmationResponseDTO>(400, "Only pending cancellation requests can be updated.");
//                    }

//                    // Update the cancellation status and metadata
//                    cancellation.Status = statusUpdate.Status;
//                    cancellation.ProcessedAt = DateTime.UtcNow;
//                    cancellation.ProcessedBy = statusUpdate.ProcessedBy;
//                    cancellation.Remarks = statusUpdate.Remarks;

//                    if (statusUpdate.Status == CancellationStatus.Approved)
//                    {
//                        // Update the order status to Canceled
//                        cancellation.Order.OrderStatus = OrderStatus.Canceled;
//                        cancellation.CancellationCharges = statusUpdate.CancellationCharges;

//                        // Restore stock quantities for each order item
//                        var orderItems = await _context.OrderItems
//                            .Include(oi => oi.Product)
//                            .Where(oi => oi.OrderId == cancellation.OrderId)
//                            .ToListAsync();

//                        foreach (var item in orderItems)
//                        {
//                            item.Product.StockQuantity += item.Quantity;
//                            _context.Products.Update(item.Product);
//                        }
//                    }

//                    await _context.SaveChangesAsync();
//                    await transaction.CommitAsync();

//                    // Optionally, notify the customer and admin about the status update
//                    // Integrate your notification/email service as needed.

//                    if (statusUpdate.Status == CancellationStatus.Approved)
//                    {
//                        await NotifyCancellationAcceptedAsync(cancellation);
//                    }
//                    else if (statusUpdate.Status == CancellationStatus.Rejected)
//                    {
//                        await NotifyCancellationRejectionAsync(cancellation);
//                    }

//                    var confirmation = new ConfirmationResponseDTO
//                    {
//                        Message = $"Cancellation request with ID {cancellation.Id} has been {cancellation.Status}."
//                    };

//                    return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
//                }
//                catch (Exception ex)
//                {
//                    await transaction.RollbackAsync();
//                    return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
//                }
//            }
//        }

//        // Retrieves all cancellation requests used by Admin.
//        public async Task<ApiResponse<List<CancellationResponseDTO>>> GetAllCancellationsAsync()
//        {
//            try
//            {
//                var cancellations = await _context.Cancellations
//                    .AsNoTracking()
//                    .Include(c => c.Order)
//                    .ToListAsync();

//                var cancellationList = cancellations.Select(c => new CancellationResponseDTO
//                {
//                    Id = c.Id,
//                    OrderId = c.OrderId,
//                    Reason = c.Reason,
//                    Status = c.Status,
//                    RequestedAt = c.RequestedAt,
//                    ProcessedAt = c.ProcessedAt,
//                    ProcessedBy = c.ProcessedBy,
//                    OrderAmount = c.OrderAmount,
//                    CancellationCharges = c.CancellationCharges,
//                    Remarks = c.Remarks,
//                }).ToList();

//                return new ApiResponse<List<CancellationResponseDTO>>(200, cancellationList);
//            }
//            catch (Exception ex)
//            {
//                return new ApiResponse<List<CancellationResponseDTO>>(500, $"An unexpected error occurred: {ex.Message}");
//            }
//        }

//        // Notify customers about cancellation status changes.
//        private async Task NotifyCancellationAcceptedAsync(Cancellation cancellation)
//        {
//            // Ensure the cancellation object has an associated order and customer.
//            if (cancellation.Order == null || cancellation.Order.Customer == null)
//            {
//                return;
//            }

//            // Build the email subject
//            string subject = $"Cancellation Request Update - Order #{cancellation.Order.OrderNumber}";

//            // Build the HTML email body using string interpolation.
//            // Adjust colors and content as needed.
//            string emailBody = $@"
//            <html>
//              <head>
//                <meta charset='UTF-8'>
//              </head>
//              <body style='font-family: Arial, sans-serif; background-color: #f0f8ff; margin: 0; padding: 20px;'>
//                <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border: 1px solid #cccccc;'>
//                  <!-- Header -->
//                  <div style='background-color: #dc3545; padding: 15px; text-align: center; color: #ffffff;'>
//                    <h2 style='margin: 0;'>Cancellation Request {cancellation.Status}</h2>
//                  </div>
      
//                  <!-- Greeting -->
//                  <p style='margin: 20px 0 5px 0;'>Dear {cancellation.Order.Customer.FirstName} {cancellation.Order.Customer.LastName},</p>
      
//                  <!-- Cancellation Details -->
//                  <p style='margin: 5px 0 20px 0;'>Your cancellation request for Order <strong>#{cancellation.Order.OrderNumber}</strong> has been <span style='color: #dc3545; font-weight: bold;'>{cancellation.Status}</span>.</p>
      
//                  <h3 style='color: #dc3545; border-bottom: 2px solid #eeeeee; padding-bottom: 5px;'>Cancellation Details</h3>
//                  <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Order Number:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.Order.OrderNumber}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Cancellation Reason:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.Reason}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Admin Remark:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.Remarks}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Requested At:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.RequestedAt:MMMM dd, yyyy HH:mm}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Processed At:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{(cancellation.ProcessedAt.HasValue ? cancellation.ProcessedAt.Value.ToString("MMMM dd, yyyy HH:mm") : "N/A")}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Order Amount:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.OrderAmount}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Cancellation Charges:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.CancellationCharges}</td>
//                    </tr>
//                    <tr>
//                      <td style='padding: 8px; background-color: #f8f8f8; border: 1px solid #dddddd;'><strong>Amount to be Refunded:</strong></td>
//                      <td style='padding: 8px; border: 1px solid #dddddd;'>{cancellation.OrderAmount - (cancellation.CancellationCharges ?? 0)}</td>
//                    </tr>
//                  </table>
                   
//                 <!-- Footer -->
//                 <div style='background-color:#f1f3f5; padding:15px; text-align:center; font-size:14px; color:#6c757d; margin-top:20px;'>
//                    <p style='margin:0;'>Thank you for choosing Our E-Commerce Store.</p>
//                    <div style='margin-top:10px;'>
//                      <a href='https://facebook.com' style='text-decoration:none; margin:0 5px;'>
//                        <img src='https://cdn-icons-png.flaticon.com/512/733/733547.png' alt='Facebook' style='width:32px;'>
//                      </a>
//                      <a href='https://twitter.com' style='text-decoration:none; margin:0 5px;'>
//                        <img src='https://cdn-icons-png.flaticon.com/512/733/733579.png' alt='Twitter' style='width:32px;'>
//                      </a>
//                      <a href='https://instagram.com' style='text-decoration:none; margin:0 5px;'>
//                        <img src='https://cdn-icons-png.flaticon.com/512/733/733558.png' alt='Instagram' style='width:32px;'>
//                      </a>
//                        <a href=""https://youtube.com"" style=""text-decoration:none; margin:0 5px;"">
//                          <img src=""https://cdn-icons-png.flaticon.com/512/1384/1384060.png"" alt=""YouTube"" style=""width:32px;"">
//                        </a>
//                        <a href=""https://linkedin.com"" style=""text-decoration:none; margin:0 5px;"">
//                          <img src=""https://cdn-icons-png.flaticon.com/512/174/174857.png"" alt=""LinkedIn"" style=""width:32px;"">
//                        </a>
//                        <a href=""https://telegram.org"" style=""text-decoration:none; margin:0 5px;"">
//                          <img src=""https://cdn-icons-png.flaticon.com/512/2111/2111646.png"" alt=""Telegram"" style=""width:32px;"">
//                        </a>
//                    </div>
//                  </div>

//                </div>
//              </body>
//            </html>";

//            // Send the email using the EmailService.
//            await _emailService.SendEmailAsync(cancellation.Order.Customer.Email, subject, emailBody, IsBodyHtml: true);
//        }

//        // Notify customers about a cancellation rejection using a distinct email design.
//        private async Task NotifyCancellationRejectionAsync(Cancellation cancellation)
//        {
//            // Ensure the cancellation object has an associated order and customer.
//            if (cancellation.Order == null || cancellation.Order.Customer == null)
//            {
//                return;
//            }

//            // Build the email subject.
//            string subject = $"Cancellation Request Rejected - Order #{cancellation.Order.OrderNumber}";

//            // Build the HTML email body using inline CSS.
//            string emailBody = $@"
//            <html>
//              <head>
//                <meta charset='UTF-8'>
//              </head>
//              <body style='font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 20px;'>
//                <div style='max-width:600px; margin:auto; background-color:#ffffff; padding:20px; border-radius:8px; box-shadow:0 4px 8px rgba(0,0,0,0.1); overflow:hidden;'>
//                  <!-- Header -->
//                  <div style='background-color:#ffc107; padding:20px; text-align:center;'>
//                    <h2 style='margin:0; color:#212529; font-size:26px;'>Cancellation Request Rejected</h2>
//                  </div>
      
//                  <!-- Content -->
//                  <div style='padding:20px; color:#343a40;'>
//                    <p style='margin:15px 0; line-height:1.6;'>Dear {cancellation.Order.Customer.FirstName} {cancellation.Order.Customer.LastName},</p>
//                    <p style='margin:15px 0; line-height:1.6;'>
//                      We regret to inform you that your cancellation request for Order <strong>#{cancellation.Order.OrderNumber}</strong> has been 
//                      <strong style='color:#dc3545;'>Rejected</strong>.
//                    </p>
        
//                    <h3 style='color:#dc3545; margin-bottom:10px;'>Rejection Details</h3>
//                    <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
//                      <tr>
//                        <th style='border:1px solid #dee2e6; padding:12px; text-align:left; background-color:#e9ecef;'>Order Number</th>
//                        <td style='border:1px solid #dee2e6; padding:12px; text-align:left;'>{cancellation.Order.OrderNumber}</td>
//                      </tr>
//                      <tr>
//                        <th style='border:1px solid #dee2e6; padding:12px; text-align:left; background-color:#e9ecef;'>Cancellation Reason</th>
//                        <td style='border:1px solid #dee2e6; padding:12px; text-align:left;'>{cancellation.Reason}</td>
//                      </tr>
//                      <tr>
//                        <th style='border:1px solid #dee2e6; padding:12px; text-align:left; background-color:#e9ecef;'>Rejection Reason</th>
//                        <td style='border:1px solid #dee2e6; padding:12px; text-align:left;'>{cancellation.Remarks}</td>
//                      </tr>
//                      <tr>
//                        <th style='border:1px solid #dee2e6; padding:12px; text-align:left; background-color:#e9ecef;'>Requested At</th>
//                        <td style='border:1px solid #dee2e6; padding:12px; text-align:left;'>{cancellation.RequestedAt:MMMM dd, yyyy HH:mm}</td>
//                      </tr>
//                      <tr>
//                        <th style='border:1px solid #dee2e6; padding:12px; text-align:left; background-color:#e9ecef;'>Processed At</th>
//                        <td style='border:1px solid #dee2e6; padding:12px; text-align:left;'>{(cancellation.ProcessedAt.HasValue ? cancellation.ProcessedAt.Value.ToString("MMMM dd, yyyy HH:mm") : "N/A")}</td>
//                      </tr>
//                    </table>
        
//                    <p style='margin:15px 0; line-height:1.6;'>If you have any questions or need further clarification, please do not hesitate to contact our support team.</p>
//                    <a href='mailto:info@dotnettutorials.net' style='display:inline-block; padding:12px 24px; margin-top:20px; background-color:#dc3545; color:#ffffff; text-decoration:none; border-radius:4px; font-weight:bold;'>Contact Support</a>
//                  </div>
      
//                  <!-- Footer -->
//                  <div style='background-color:#f1f3f5; padding:15px; text-align:center; font-size:14px; color:#6c757d; margin-top:20px;'>
//                    <p style='margin:0;'>Thank you for choosing Our E-Commerce Store.</p>
//                    <div style='margin-top:10px;'>
//                      <a href='https://facebook.com' style='text-decoration:none; margin:0 5px;'>
//                        <img src='https://cdn-icons-png.flaticon.com/512/733/733547.png' alt='Facebook' style='width:32px;'>
//                      </a>
//                      <a href='https://twitter.com' style='text-decoration:none; margin:0 5px;'>
//                        <img src='https://cdn-icons-png.flaticon.com/512/733/733579.png' alt='Twitter' style='width:32px;'>
//                      </a>
//                      <a href='https://instagram.com' style='text-decoration:none; margin:0 5px;'>
//                        <img src='https://cdn-icons-png.flaticon.com/512/733/733558.png' alt='Instagram' style='width:32px;'>
//                      </a>
//                    <a href=""https://youtube.com"" style=""text-decoration:none; margin:0 5px;"">
//                      <img src=""https://cdn-icons-png.flaticon.com/512/1384/1384060.png"" alt=""YouTube"" style=""width:32px;"">
//                    </a>
//                    <a href=""https://linkedin.com"" style=""text-decoration:none; margin:0 5px;"">
//                      <img src=""https://cdn-icons-png.flaticon.com/512/174/174857.png"" alt=""LinkedIn"" style=""width:32px;"">
//                    </a>
//                    <a href=""https://telegram.org"" style=""text-decoration:none; margin:0 5px;"">
//                      <img src=""https://cdn-icons-png.flaticon.com/512/2111/2111646.png"" alt=""Telegram"" style=""width:32px;"">
//                    </a>
//                    </div>
//                  </div>
//                </div>
//              </body>
//            </html>";

//            // Send the email using the EmailService.
//            await _emailService.SendEmailAsync(cancellation.Order.Customer.Email, subject, emailBody, IsBodyHtml: true);
//        }
//    }
//}