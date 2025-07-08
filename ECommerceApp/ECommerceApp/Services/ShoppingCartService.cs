using Azure;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.DTOs.ShoppingCartDTOs;
using ECommerceApp.Models;
using ECommerceApp.Repository;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services
{
    public class ShoppingCartService
    {
        // Dependency injection of the ApplicationDbContext to interact with the database.
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IProductRepository _productRepository;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, IProductRepository productRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _productRepository = productRepository;
        }

        // Retrieves the active (non-checked-out) cart for a given customer.
        // If no active cart exists, an empty cart (with price details set to 0) is returned.
        public async Task<ApiResponse<CartResponseDTO>> GetCartByCustomerIdAsync(int customerId)
        {
            try
            {
                // Query the database for a cart that belongs to the specified customer and is not checked out.
                var cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(customerId);

                if (cart == null)
                {
                    var emptyCartDTO = new CartResponseDTO
                    {
                        CustomerId = customerId,
                        IsCheckedOut = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItemResponseDTO>(),
                        TotalBasePrice = 0,
                        TotalDiscount = 0,
                        TotalAmount = 0
                    };

                    return new ApiResponse<CartResponseDTO>(200, emptyCartDTO);
                }

                return new ApiResponse<CartResponseDTO>(200, cart);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CartResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Adds a product to the customer's cart.
        // Creates an active cart if one does not exist.
        // If the product already exists in the cart, its quantity is updated.
        public async Task<ApiResponse<bool>> AddToCartAsync(AddToCartDTO addToCartDTO)
        {
            try
            {
                var product = await _productRepository.GetProductById(addToCartDTO.ProductId);
                if (product == null)
                {
                    return new ApiResponse<bool>(404, "Product not found.");
                }

                //var cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(addToCartDTO.CustomerId);

                var discount = product.DiscountPercentage > 0 ? product.Price * product.DiscountPercentage / 100 : 0;


                    //var cartRequest = new Cart
                    //{
                    //    Id = cart.Id,
                    //    CustomerId = addToCartDTO.CustomerId,
                    //    IsCheckedOut = false,
                    //    CreatedAt = DateTime.UtcNow,
                    //    UpdatedAt = DateTime.UtcNow,
                    //    CartItems = new List<CartItem> {
                    //        new CartItem
                    //         {
                    //            Id = 0 ,
                    //             CartId = cart.Id,
                    //             ProductId = product.Id,
                    //             Quantity = addToCartDTO.Quantity,
                    //             UnitPrice = product.Price,
                    //             Discount = product.DiscountPercentage > 0 ? product.Price * product.DiscountPercentage / 100 : 0,
                    //             TotalPrice = (product.Price - discount) * addToCartDTO.Quantity,
                    //             CreatedAt = DateTime.UtcNow,
                    //             UpdatedAt = DateTime.UtcNow
                    //         }
                    //    }
                    //};

                    var result = await _shoppingCartRepository.AddToCartAsync(addToCartDTO);
                    return new ApiResponse<bool>(200, result);


                //else
                //{
                //    var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addToCartDTO.ProductId);
                //    if (existingCartItem != null)
                //    {
                //        if (existingCartItem.Quantity + addToCartDTO.Quantity > product.StockQuantity)
                //        {
                //            return new ApiResponse<bool>(400, $"Adding {addToCartDTO.Quantity} exceeds available stock.");
                //        }

                //        CartItem cartItemRequest = new CartItem()
                //        {
                //            Id = existingCartItem.Id,
                //            CartId = cart.Id,
                //            ProductId = existingCartItem.ProductId,
                //            UnitPrice = existingCartItem.UnitPrice,
                //            Discount = existingCartItem.Discount,
                //            Quantity = existingCartItem.Quantity += addToCartDTO.Quantity,
                //            CreatedAt = DateTime.UtcNow,
                //            UpdatedAt = DateTime.UtcNow,
                //            TotalPrice = (existingCartItem.UnitPrice - existingCartItem.Discount) * existingCartItem.Quantity,
                //        };
                //        var result = await _shoppingCartRepository.UpdateCartItemAsync(cartItemRequest);
                //        return new ApiResponse<bool>(200, result);
                //    }
                //    else
                //    {
                //        var cartRequest = new Cart
                //        {
                //            Id = cart.Id,
                //            CustomerId = addToCartDTO.CustomerId,
                //            IsCheckedOut = false,
                //            CreatedAt = DateTime.UtcNow,
                //            UpdatedAt = DateTime.UtcNow,
                //            CartItems = new List<CartItem> {
                //            new CartItem
                //             {
                //                 Id = 0,
                //                 CartId = cart.Id,
                //                 ProductId = product.Id,
                //                 Quantity = addToCartDTO.Quantity,
                //                 UnitPrice = product.Price,
                //                 Discount = product.DiscountPercentage > 0 ? product.Price * product.DiscountPercentage / 100 : 0,
                //                 TotalPrice = (product.Price - discount) * addToCartDTO.Quantity,
                //                 CreatedAt = DateTime.UtcNow,
                //                 UpdatedAt = DateTime.UtcNow
                //             }
                //        }
                //        };

                //        var result = await _shoppingCartRepository.AddToCartAsync(cartRequest);
                //        return new ApiResponse<bool>(200, result);
                //    }
                //}
                //// Update the cart's last updated timestamp.
                //cart.UpdatedAt = DateTime.UtcNow;
                //_context.Carts.Update(cart);
                //await _context.SaveChangesAsync();

                //// Reload the cart with the latest details (including related CartItems and Products).
                //cart = await _context.Carts
                //    .Include(c => c.CartItems)
                //        .ThenInclude(ci => ci.Product)
                //    .FirstOrDefaultAsync(c => c.Id == cart.Id) ?? new Cart();

                //// Map the cart entity to the DTO, which includes price calculations.
                //var cartDTO = MapCartToDTO(cart);
                //return new ApiResponse<CartResponseDTO>(200, cartDTO);
            }
            catch (Exception ex)
            {
                // Return error response in case of exceptions.
                return new ApiResponse<bool>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Updates the quantity of a specific item in the customer's cart.
        public async Task<ApiResponse<bool>> UpdateCartItemAsync(UpdateCartItemDTO updateCartItemDTO)
        {
            try
            {
                var result = await _shoppingCartRepository.UpdateCartItemAsync(updateCartItemDTO);
                return new ApiResponse<bool>(200, result);
            }
            catch (Exception ex)
            {
                // Return error response if an exception occurs.
                return new ApiResponse<bool>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        //// Removes a specific item from the customer's cart.
        //public async Task<ApiResponse<CartResponseDTO>> RemoveCartItemAsync(RemoveCartItemDTO removeCartItemDTO)
        //{
        //    try
        //    {
        //        // Retrieve the active cart along with its items and product details.
        //        var cart = await _context.Carts
        //            .Include(c => c.CartItems)
        //                .ThenInclude(ci => ci.Product)
        //            .FirstOrDefaultAsync(c => c.CustomerId == removeCartItemDTO.CustomerId && !c.IsCheckedOut);

        //        // Return 404 if no active cart is found.
        //        if (cart == null)
        //        {
        //            return new ApiResponse<CartResponseDTO>(404, "Active cart not found.");
        //        }

        //        // Find the cart item to remove.
        //        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == removeCartItemDTO.CartItemId);
        //        if (cartItem == null)
        //        {
        //            return new ApiResponse<CartResponseDTO>(404, "Cart item not found.");
        //        }

        //        // Remove the cart item from the context.
        //        _context.CartItems.Remove(cartItem);
        //        cart.UpdatedAt = DateTime.UtcNow;
        //        await _context.SaveChangesAsync();

        //        // Reload the updated cart after removal.
        //        cart = await _context.Carts
        //            .Include(c => c.CartItems)
        //                .ThenInclude(ci => ci.Product)
        //            .FirstOrDefaultAsync(c => c.Id == cart.Id) ?? new Cart();

        //        // Map the updated cart to the DTO.
        //        var cartDTO = MapCartToDTO(cart);
        //        return new ApiResponse<CartResponseDTO>(200, cartDTO);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return error response if an exception occurs.
        //        return new ApiResponse<CartResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
        //    }
        //}

        //// Clears all items from the customer's active cart.
        //public async Task<ApiResponse<ConfirmationResponseDTO>> ClearCartAsync(int customerId)
        //{
        //    try
        //    {
        //        // Retrieve the active cart along with its items.
        //        var cart = await _context.Carts
        //            .Include(c => c.CartItems)
        //            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

        //        // Return 404 if no active cart is found.
        //        if (cart == null)
        //        {
        //            return new ApiResponse<ConfirmationResponseDTO>(404, "Active cart not found.");
        //        }

        //        // If there are any items in the cart, remove them.
        //        if (cart.CartItems.Any())
        //        {
        //            _context.CartItems.RemoveRange(cart.CartItems);
        //            cart.UpdatedAt = DateTime.UtcNow;
        //            await _context.SaveChangesAsync();
        //        }

        //        // Create a confirmation response DTO.
        //        var confirmation = new ConfirmationResponseDTO
        //        {
        //            Message = "Cart has been cleared successfully."
        //        };

        //        return new ApiResponse<ConfirmationResponseDTO>(200, confirmation);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return error response if an exception occurs.
        //        return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
        //    }
        //}

        //// Helper method to manually map a Cart entity to a CartResponseDTO.
        //// This method also calculates the TotalBasePrice, TotalDiscount, and TotalAmount.
        //private CartResponseDTO MapCartToDTO(Cart cart)
        //{
        //    // Map each CartItem entity to its corresponding CartItemResponseDTO.
        //    var cartItemsDto = cart.CartItems?.Select(ci => new CartItemResponseDTO
        //    {
        //        Id = ci.Id,
        //        ProductId = ci.ProductId,
        //        ProductName = ci.Product?.Name,
        //        Quantity = ci.Quantity,
        //        UnitPrice = ci.UnitPrice,
        //        Discount = ci.Discount,
        //        TotalPrice = ci.TotalPrice
        //    }).ToList() ?? new List<CartItemResponseDTO>();

        //    // Initialize totals for base price, discount, and amount after discount.
        //    decimal totalBasePrice = 0;
        //    decimal totalDiscount = 0;
        //    decimal totalAmount = 0;

        //    // Iterate through each cart item DTO to accumulate the totals.
        //    foreach (var item in cartItemsDto)
        //    {
        //        totalBasePrice += item.UnitPrice * item.Quantity;       // Sum of base prices (without discount)
        //        totalDiscount += item.Discount * item.Quantity;         // Sum of discounts applied per unit
        //        totalAmount += item.TotalPrice;                         // Sum of final prices after discount
        //    }

        //    // Create and return the final CartResponseDTO with all details and calculated totals.
        //    return new CartResponseDTO
        //    {
        //        Id = cart.Id,
        //        CustomerId = cart.CustomerId,
        //        IsCheckedOut = cart.IsCheckedOut,
        //        CreatedAt = cart.CreatedAt,
        //        UpdatedAt = cart.UpdatedAt,
        //        CartItems = cartItemsDto,
        //        TotalBasePrice = totalBasePrice,
        //        TotalDiscount = totalDiscount,
        //        TotalAmount = totalAmount
        //    };
        //}
    }
}