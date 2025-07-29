using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.DTOs.OrderDTOs;
using ECommerceApp.DTOs.ProductDTOs;
using ECommerceApp.Models;

namespace ECommerceApp.Repository
{
    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(Order orderDto);

        Task<OrderResponseDTO> GetOrderByIdAsync(int Id);

    }
    public class OrderRepository : IOrderRepository
    {
        private readonly IDapperDbConnection _dbContext;

        public OrderRepository(IDapperDbConnection dbContext) => _dbContext = dbContext;
        public async Task<int> CreateOrderAsync(Order orderDto)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();

            var orderId = 0;
            try
            {
                // Generate a unique order number
                var orderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + new Random().Next(1000, 9999);

                var insertOrderQuery = @"
            INSERT INTO Orders 
            (OrderNumber, OrderDate, CustomerId, BillingAddressId, ShippingAddressId, TotalBaseAmount, TotalDiscountAmount, ShippingCost, TotalAmount, OrderStatus)
            OUTPUT INSERTED.Id
            VALUES 
            (@OrderNumber, @OrderDate, @CustomerId, @BillingAddressId, @ShippingAddressId, @TotalBaseAmount, @TotalDiscountAmount, @ShippingCost, @TotalAmount, @OrderStatus)";

                 orderId = await dbConnection.ExecuteScalarAsync<int>(insertOrderQuery, new
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.UtcNow,
                    orderDto.CustomerId,
                    orderDto.BillingAddressId,
                    orderDto.ShippingAddressId,
                    orderDto.TotalBaseAmount,
                    orderDto.TotalDiscountAmount,
                    orderDto.ShippingCost,
                    orderDto.TotalAmount,
                    orderDto.OrderStatus
                }, transaction);

                foreach (var orderItem in orderDto.OrderItems)
                {
                    var orderItemQuery = @"
                INSERT INTO OrderItems 
                (OrderId, ProductId, Quantity, UnitPrice, Discount, TotalPrice)
                VALUES 
                (@OrderId, @ProductId, @Quantity, @UnitPrice, @Discount, @TotalPrice)";

                    await dbConnection.ExecuteAsync(orderItemQuery, new
                    {
                        OrderId = orderId,
                        orderItem.ProductId,
                        orderItem.Quantity,
                        orderItem.UnitPrice,
                        orderItem.Discount,
                        orderItem.TotalPrice
                    }, transaction);
                }

                transaction.Commit();
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<OrderResponseDTO> GetOrderByIdAsync(int Id)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            try
            {
                var sQuery = "select * from Orders where ID = @Id";
                return await dbConnection.QuerySingleAsync<OrderResponseDTO>(sQuery, new { Id });
            }
            catch
            {
                throw;
            }
        }

    }
}
