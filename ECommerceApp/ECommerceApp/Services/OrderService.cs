using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.DTOs.OrderDTOs;
using ECommerceApp.DTOs.ProductDTOs;
using ECommerceApp.Models;
using ECommerceApp.Repository;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Cryptography;

namespace ECommerceApp.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;


        private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedStatusTransitions = new()
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Canceled } },
            { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Canceled } },
            { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered } },
            { OrderStatus.Delivered, new List<OrderStatus>() }, 
            { OrderStatus.Canceled, new List<OrderStatus>() }   
        };

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, IShoppingCartRepository shoppingCartRepository)
        {
            _orderRepository = orderRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _productRepository = productRepository;
        }

        public async Task<ApiResponse<int>> CreateOrderAsync(OrderCreateDTO orderDto)
        {
            try
            {

                decimal totalBaseAmount = 0;
                decimal totalDiscountAmount = 0;
                decimal shippingCost = 10.00m; 
                decimal totalAmount = 0;


                var orderItems = new List<OrderItem>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var product = await _productRepository.GetProductById(itemDto.ProductId);
                    if (product == null)
                    {
                        return new ApiResponse<int>(404, $"Product with ID {itemDto.ProductId} does not exist.");
                    }

                    if (product.StockQuantity < itemDto.Quantity)
                    {
                        return new ApiResponse<int>(400, $"Insufficient stock for product {product.Name}.");
                    }

                    decimal basePrice = itemDto.Quantity * product.Price;
                    decimal discount = (product.DiscountPercentage / 100.0m) * basePrice;
                    decimal totalPrice = basePrice - discount;

                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Discount = discount,
                        TotalPrice = totalPrice
                    };

                    orderItems.Add(orderItem);

                    totalBaseAmount += basePrice;
                    totalDiscountAmount += discount;

                    product.StockQuantity -= itemDto.Quantity;
                    var updateProduct = new ProductUpdateDTO 
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        StockQuantity = product.StockQuantity,
                        Price = product.Price,
                        DiscountPercentage = product.DiscountPercentage,
                        CategoryId = product.CategoryId,
                    };

                    await _productRepository.UpdateProduct(updateProduct);
                }

                totalAmount = totalBaseAmount - totalDiscountAmount + shippingCost;

                var order = new Order
                {
                    OrderNumber = $"ORD-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}-{RandomNumber(1000, 9999)}",
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    BillingAddressId = orderDto.BillingAddressId,
                    ShippingAddressId = orderDto.ShippingAddressId, 
                    TotalBaseAmount = totalBaseAmount,
                    TotalDiscountAmount = totalDiscountAmount,
                    ShippingCost = shippingCost,
                    TotalAmount = totalAmount,
                    OrderStatus = OrderStatus.Pending,
                    OrderItems = orderItems
                };

                // Add the order to the database.


                // Mark the customer's active cart as checked out (if it exists).
                var cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(orderDto.CustomerId);
                if (cart != null)
                {
                    await _shoppingCartRepository.UpdateCartAsync(cart.Id);
                }

                var orderResponse = _orderRepository.CreateOrderAsync(order);
                return new ApiResponse<int>(200, orderResponse.Result);
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Retrieves an order by its ID along with related entities.
        public async Task<ApiResponse<OrderResponseDTO>> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return new ApiResponse<OrderResponseDTO>(200, await _orderRepository.GetOrderByIdAsync(orderId));
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        //    // Updates the status of an existing order.
        //    // Validates allowed status transitions before applying the update.
        //    public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateOrderStatusAsync(OrderStatusUpdateDTO statusDto)
        //    {
        //        try
        //        {
        //            // Retrieve the order.
        //            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == statusDto.OrderId);
        //            if (order == null)
        //            {
        //                return new ApiResponse<ConfirmationResponseDTO>(404, "Order not found.");
        //            }

        //            var currentStatus = order.OrderStatus;
        //            var newStatus = statusDto.OrderStatus;

        //            // Validate the status transition.
        //            if (!AllowedStatusTransitions.TryGetValue(currentStatus, out var allowedStatuses))
        //            {
        //                return new ApiResponse<ConfirmationResponseDTO>(500, "Current order status is invalid.");
        //            }
        //            if (!allowedStatuses.Contains(newStatus))
        //            {
        //                return new ApiResponse<ConfirmationResponseDTO>(400, $"Cannot change order status from {currentStatus} to {newStatus}.");
        //            }

        //            // Update the order status.
        //            order.OrderStatus = newStatus;
        //            await _context.SaveChangesAsync();

        //            // Prepare a confirmation message.
        //            var confirmation = new ConfirmationResponseDTO
        //            {
        //                Message = $"Order Status with Id {statusDto.OrderId} updated successfully."
        //            };

        //            return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
        //        }
        //    }

        //    // Retrieves all orders in the system.
        //    public async Task<ApiResponse<List<OrderResponseDTO>>> GetAllOrdersAsync()
        //    {
        //        try
        //        {
        //            // Retrieve all orders with related entities.
        //            var orders = await _context.Orders
        //                .Include(o => o.OrderItems)
        //                    .ThenInclude(oi => oi.Product)
        //                .Include(o => o.Customer)
        //                .Include(o => o.BillingAddress)
        //                .Include(o => o.ShippingAddress)
        //                .AsNoTracking()
        //                .ToListAsync();

        //            // Map each order to its corresponding DTO.
        //            var orderList = orders.Select(o => MapOrderToDTO(o, o.Customer, o.BillingAddress, o.ShippingAddress)).ToList();
        //            return new ApiResponse<List<OrderResponseDTO>>(200, orderList);
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ApiResponse<List<OrderResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
        //        }
        //    }

        //    // Retrieves all orders associated with a specific customer.
        //    public async Task<ApiResponse<List<OrderResponseDTO>>> GetOrdersByCustomerAsync(int customerId)
        //    {
        //        try
        //        {
        //            // Retrieve the customer along with their orders and related data.
        //            var customer = await _context.Customers
        //                .Include(c => c.Orders)
        //                    .ThenInclude(o => o.OrderItems)
        //                        .ThenInclude(oi => oi.Product)
        //                .Include(c => c.Addresses)
        //                .AsNoTracking()
        //                .FirstOrDefaultAsync(c => c.Id == customerId);

        //            if (customer == null)
        //            {
        //                return new ApiResponse<List<OrderResponseDTO>>(404, "Customer not found.");
        //            }

        //            // Map each order to a DTO.
        //            var orders = customer.Orders.Select(o => MapOrderToDTO(o, customer, o.BillingAddress, o.ShippingAddress)).ToList();
        //            return new ApiResponse<List<OrderResponseDTO>>(200, orders);
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ApiResponse<List<OrderResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
        //        }
        //    }

        //    #region Helper Methods

        //    // Maps an Order entity to an OrderResponseDTO.
        //    private OrderResponseDTO MapOrderToDTO(Order order, Customer customer, Address billingAddress, Address shippingAddress)
        //    {
        //        // Map order items.
        //        var orderItemsDto = order.OrderItems.Select(oi => new OrderItemResponseDTO
        //        {
        //            Id = oi.Id,
        //            ProductId = oi.ProductId,
        //            Quantity = oi.Quantity,
        //            UnitPrice = oi.UnitPrice,
        //            Discount = oi.Discount,
        //            TotalPrice = oi.TotalPrice
        //        }).ToList();

        //        // Create and return the DTO.
        //        return new OrderResponseDTO
        //        {
        //            Id = order.Id,
        //            OrderNumber = order.OrderNumber,
        //            OrderDate = order.OrderDate,
        //            CustomerId = order.CustomerId,
        //            BillingAddressId = order.BillingAddressId,
        //            ShippingAddressId = order.ShippingAddressId,
        //            TotalBaseAmount = order.TotalBaseAmount,
        //            TotalDiscountAmount = order.TotalDiscountAmount,
        //            ShippingCost = order.ShippingCost,
        //            TotalAmount = Math.Round(order.TotalAmount, 2),
        //            OrderStatus = order.OrderStatus,
        //            OrderItems = orderItemsDto
        //        };
        //    }

        //    // Generates a unique order number using the current UTC date/time and a random number.
        //    // Format: ORD-yyyyMMdd-HHmmss-XXXX
        //    private string GenerateOrderNumber()
        //    {
        //        return $"ORD-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}-{RandomNumber(1000, 9999)}";
        //    }

        //    // Generates a random number between min and max.
        private int RandomNumber(int min, int max)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return Math.Abs(BitConverter.ToInt32(bytes, 0) % (max - min + 1)) + min;
            }
        }

        //    #endregion
    }
}