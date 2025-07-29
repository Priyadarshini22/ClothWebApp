using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.DTOs.PaymentDTOs;
using ECommerceApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // For IConfiguration
using Stripe;
using System.Data; // For IDbConnection, IDbTransaction


namespace ECommerceApp.Repositories
{

        public interface IPaymentRepository
        {
        // CRUD operations for Payment
        Task<ApiResponse<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO paymentRequest);
            Task<Payment> GetPaymentByIdAsync(int paymentId);
            Task<Payment> GetPaymentByOrderIdAsync(int orderId);
            Task<int> AddPaymentAsync(Payment payment); // Returns the ID of the new payment
            Task UpdatePaymentAsync(Payment payment);

            // Order-related queries needed for payment processing
            Task<Order> GetOrderForPaymentAsync(int orderId, int customerId);
            Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

            // For email service, fetch a more complete order details
        }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDapperDbConnection _dbContext;
        private readonly IConfiguration _configuration;

        public PaymentRepository(IDapperDbConnection dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"]; // ✅ Fix here
           // Optional to inject if needed
        }

        public async Task<ApiResponse<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO paymentRequest)
        {
            using var connection = _dbContext.CreateConnection();
           connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Retrieve order with existing payment (if any)
                var orderSql = @"
          SELECT o.Id, o.CustomerId, o.TotalAmount, o.OrderStatus,
                 p.Id, p.OrderId, p.Amount, p.Status, p.PaymentMethod
          FROM Orders o
          LEFT JOIN Payments p ON p.OrderId = o.Id
          WHERE o.Id = @OrderId AND o.CustomerId = @CustomerId";

                var orderDict = new Dictionary<int, (Order order, Payment payment)>();
                var result = await connection.QueryAsync<Order, Payment, (Order, Payment)>(
                    orderSql,
                    (o, p) =>
                    {
                        if (!orderDict.TryGetValue(o.Id, out var tuple))
                        {
                            tuple = (o, null);
                            orderDict[o.Id] = tuple;
                        }
                        if (p != null) tuple.payment = p;
                        return tuple;
                    },
                    new { paymentRequest.OrderId, paymentRequest.CustomerId },
                    transaction,
                    splitOn: "Id"
                );

                var entry = orderDict.Values.FirstOrDefault();
                if (entry.order == null)
                    return new ApiResponse<PaymentResponseDTO>(404, "Order not found.");

                var order = entry.order;
                order.Payment = entry.payment;

                // 2. Amount validation
                if (Math.Round(paymentRequest.Amount, 2) != Math.Round(order.TotalAmount, 2))
                    return new ApiResponse<PaymentResponseDTO>(400, "Amount mismatch.");

                // 3. Insert or update Payment
                Payment paymentEntity = entry.payment;
                if (paymentEntity != null)
                {
                    if (paymentEntity.Status != PaymentStatus.Failed || order.OrderStatus != OrderStatus.Pending)
                        return new ApiResponse<PaymentResponseDTO>(400, "Already paid or non-retryable payment.");

                    paymentEntity.PaymentMethod = paymentRequest.PaymentMethod;
                    paymentEntity.Amount = paymentRequest.Amount;
                    paymentEntity.PaymentDate = DateTime.UtcNow;
                    paymentEntity.Status = PaymentStatus.Pending;

                    var updatePaymentSql = @"
              UPDATE Payments SET PaymentMethod=@PaymentMethod,
                 Amount=@Amount, PaymentDate=@PaymentDate, Status=@Status
              WHERE Id=@Id";
                    await connection.ExecuteAsync(updatePaymentSql, paymentEntity, transaction);
                }
                else
                {
                    paymentEntity = new Payment
                    {
                        OrderId = order.Id,
                        PaymentMethod = paymentRequest.PaymentId,
                        Amount = paymentRequest.Amount,
                        PaymentDate = DateTime.UtcNow,
                        Status = PaymentStatus.Pending
                    };

                    var insertSql = @"
              INSERT INTO Payments (OrderId, PaymentMethod, Amount, PaymentDate, Status)
              VALUES (@OrderId,@PaymentMethod,@Amount,@PaymentDate,@Status);
              SELECT CAST(SCOPE_IDENTITY() as int)";
                    paymentEntity.Id = await connection.ExecuteScalarAsync<int>(insertSql, paymentEntity, transaction);
                }

                var paymentMethodService = new PaymentMethodService();

                //var paymentMethodCreateOptions = new PaymentMethodCreateOptions
                //{
                //    Type = "card",
                //    Card = new PaymentMethodCardOptions { Token = paymentRequest.StripeToken }
                //};
                //var paymentMethod = await paymentMethodService.CreateAsync(paymentMethodCreateOptions);

                // 4. Process with Stripe
                if (paymentRequest.PaymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase)
                    || paymentRequest.PaymentMethod.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
                {
                    var paymentIntentService = new PaymentIntentService();

                    try
                    {
                        var paymentIntent = await paymentIntentService.GetAsync(paymentRequest.PaymentId);
                        if (paymentIntent.Status != "succeeded")
                        {
                            paymentEntity.Status = PaymentStatus.Failed;
                            await connection.ExecuteAsync(
                                "UPDATE Payments SET Status=@Status WHERE Id=@Id",
                                new { Status = paymentEntity.Status, Id = paymentEntity.Id }, transaction);

                            transaction.Commit();
                            return new ApiResponse<PaymentResponseDTO>(400, $"Stripe payment not successful: {paymentIntent.LastPaymentError?.Message ?? "Unknown error"}");
                        }

                        // Payment succeeded
                        paymentEntity.Status = PaymentStatus.Completed;
                        paymentEntity.TransactionId = paymentIntent.Id;

                        await connection.ExecuteAsync(
                            "UPDATE Payments SET Status=@Status, TransactionId=@TransactionId WHERE Id=@Id",
                            new { paymentEntity.Status, paymentEntity.TransactionId, paymentEntity.Id }, transaction);

                        order.OrderStatus = OrderStatus.Processing;
                        await connection.ExecuteAsync(
                            "UPDATE Orders SET OrderStatus=@OrderStatus WHERE Id=@Id",
                            new { OrderStatus = order.OrderStatus, Id = order.Id }, transaction);

                    }
                    catch (StripeException ex)
                    {
                        paymentEntity.Status = PaymentStatus.Failed;
                        await connection.ExecuteAsync(
                            "UPDATE Payments SET Status=@Status WHERE Id=@Id",
                            new { Id = paymentEntity.Id, Status = PaymentStatus.Failed }, transaction);
                        transaction.Commit();
                        return new ApiResponse<PaymentResponseDTO>(400, $"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
                    }
                }
                else if (paymentRequest.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    paymentEntity.Status = PaymentStatus.Completed;
                    order.OrderStatus = OrderStatus.Processing;

                    await connection.ExecuteAsync(
                        "UPDATE Payments SET Status=@Status WHERE Id=@Id",
                        new { paymentEntity.Status, paymentEntity.Id }, transaction);

                    await connection.ExecuteAsync(
                        "UPDATE Orders SET OrderStatus=@OrderStatus WHERE Id=@Id",
                        new { OrderStatus = order.OrderStatus, Id = order.Id }, transaction);
                }
                else
                {
                    return new ApiResponse<PaymentResponseDTO>(400, "Unsupported payment method");
                }

                transaction.Commit();

                var responseDto = new PaymentResponseDTO
                {
                    PaymentId = paymentEntity.Id,
                    OrderId = paymentEntity.OrderId,
                    PaymentMethod = paymentEntity.PaymentMethod,
                    TransactionId = paymentEntity.TransactionId,
                    Amount = paymentEntity.Amount,
                    PaymentDate = paymentEntity.PaymentDate,
                    Status = paymentEntity.Status
                };

                return new ApiResponse<PaymentResponseDTO>(200, responseDto);
            }
            catch (Exception ex)
            {
                try { transaction.Rollback(); } catch { }
                Console.WriteLine($"Error: {ex.Message}");
                return new ApiResponse<PaymentResponseDTO>(500, "Unexpected error during payment");
            }
        }

        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sql = @"
                SELECT Id, OrderId, PaymentMethod, Amount, PaymentDate, Status, TransactionId
                FROM Payments
                WHERE Id = @PaymentId;";
            return await dbConnection.QueryFirstOrDefaultAsync<Payment>(sql, new { PaymentId = paymentId });
        }

        public async Task<Payment> GetPaymentByOrderIdAsync(int orderId)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sql = @"
                SELECT Id, OrderId, PaymentMethod, Amount, PaymentDate, Status, TransactionId
                FROM Payments
                WHERE OrderId = @OrderId;";
            return await dbConnection.QueryFirstOrDefaultAsync<Payment>(sql, new { OrderId = orderId });
        }

        public async Task<int> AddPaymentAsync(Payment payment)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sql = @"
                INSERT INTO Payments (OrderId, PaymentMethod, Amount, PaymentDate, Status, TransactionId)
                VALUES (@OrderId, @PaymentMethod, @Amount, @PaymentDate, @Status, @TransactionId);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await dbConnection.ExecuteAsync(sql, payment); // ExecuteAsync can return single value too
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sql = @"
                UPDATE Payments
                SET PaymentMethod = @PaymentMethod, Amount = @Amount, PaymentDate = @PaymentDate,
                    Status = @Status, TransactionId = @TransactionId
                WHERE Id = @Id;";
            await dbConnection.ExecuteAsync(sql, payment);
        }

        public async Task<Order> GetOrderForPaymentAsync(int orderId, int customerId)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            // This query fetches the order and eagerly loads its associated payment if one exists.
            var sql = @"
                SELECT o.*,
                       p.Id AS PaymentId, p.PaymentMethod, p.Amount AS PaymentAmount, p.PaymentDate AS PaymentDate, p.Status AS PaymentStatus, p.TransactionId
                FROM Orders o
                LEFT JOIN Payments p ON o.Id = p.OrderId
                WHERE o.Id = @OrderId AND o.CustomerId = @CustomerId;";

            var orderDictionary = new Dictionary<int, Order>();

            var result = await _dbContext.CreateConnection().QueryAsync<Order, Payment, Order>(
                sql,
                (order, payment) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
                    {
                        existingOrder = order;
                        orderDictionary.Add(existingOrder.Id, existingOrder);
                    }
                    existingOrder.Payment = payment; // This will be null if no payment is joined
                    return existingOrder;
                },
                new { OrderId = orderId, CustomerId = customerId },
                splitOn: "PaymentId" // Column where the Payment object properties begin
            );

            return result.FirstOrDefault();
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sql = @"
                UPDATE Orders
                SET OrderStatus = @Status
                WHERE Id = @OrderId;";
            await dbConnection.ExecuteAsync(sql, new { Status = status, OrderId = orderId });
        }

        //public async Task<Order> GetOrderDetailsForEmailAsync(int orderId)
        //{
        //    // This query fetches the full order details required for the email template,
        //    // including customer, addresses, payment, order items, and associated products.
        //    var sql = @"
        //        SELECT o.*,
        //               c.Id AS Customer_Id, c.FirstName, c.LastName, c.Email,
        //               ba.Id AS BillingAddress_Id, ba.AddressLine1 AS BillingAddress_AddressLine1, ba.AddressLine2 AS BillingAddress_AddressLine2, ba.City AS BillingAddress_City, ba.State AS BillingAddress_State, ba.PostalCode AS BillingAddress_PostalCode, ba.Country AS BillingAddress_Country,
        //               sa.Id AS ShippingAddress_Id, sa.AddressLine1 AS ShippingAddress_AddressLine1, sa.AddressLine2 AS ShippingAddress_AddressLine2, sa.City AS ShippingAddress_City, sa.State AS ShippingAddress_State, sa.PostalCode AS ShippingAddress_PostalCode, sa.Country AS ShippingAddress_Country,
        //               p.Id AS Payment_Id, p.PaymentMethod, p.Amount AS Payment_Amount, p.PaymentDate AS Payment_PaymentDate, p.Status AS Payment_Status, p.TransactionId AS Payment_TransactionId,
        //               oi.Id AS OrderItem_Id, oi.ProductId, oi.Quantity, oi.UnitPrice, oi.Discount, oi.TotalPrice,
        //               prod.Id AS Product_Id, prod.Name AS Product_Name, prod.Price AS Product_Price
        //        FROM Orders o
        //        JOIN Customers c ON o.CustomerId = c.Id
        //        JOIN Addresses ba ON o.BillingAddressId = ba.Id
        //        JOIN Addresses sa ON o.ShippingAddressId = sa.Id
        //        LEFT JOIN Payments p ON o.Id = p.OrderId
        //        LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
        //        LEFT JOIN Products prod ON oi.ProductId = prod.Id
        //        WHERE o.Id = @OrderId;";

        //    var orderDictionary = new Dictionary<int, Order>();

        //    var result = await _dbContext.CreateConnection().QueryAsync<Order, Customer, Address, Address, Payment, OrderItem, Product, Order>(
        //        sql,
        //        (order, customer, billingAddress, shippingAddress, payment, orderItem, product) =>
        //        {
        //            if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
        //            {
        //                existingOrder = order;
        //                existingOrder.Customer = customer;
        //                existingOrder.BillingAddress = billingAddress;
        //                existingOrder.ShippingAddress = shippingAddress;
        //                existingOrder.Payment = payment; // This will be null if no payment is joined
        //                existingOrder.OrderItems = new List<OrderItem>();
        //                orderDictionary.Add(existingOrder.Id, existingOrder);
        //            }

        //            if (orderItem != null)
        //            {
        //                orderItem.Product = product;
        //                existingOrder.OrderItems.Add(orderItem);
        //            }

        //            return existingOrder;
        //        },
        //        new { OrderId = orderId },
        //        splitOn: "Customer_Id, BillingAddress_Id, ShippingAddress_Id, Payment_Id, OrderItem_Id, Product_Id"
        //    );

        //    return result.FirstOrDefault();
        //}
    }
}