using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs.ProductDTOs;
using ECommerceApp.DTOs.ShoppingCartDTOs;
using ECommerceApp.Models;
using ECommerceApp.Models.QueryModel;
using System.Data.Common;
using System.Linq.Expressions;

namespace ECommerceApp.Repository
{

    public interface IShoppingCartRepository
    {
        Task<bool> AddToCartAsync(Cart addToCartDTO);
        Task<bool> UpdateCartItemAsync(CartItem updateCartDTO);
        Task<CartResponseDTO> GetCartByCustomerIdAsync(int customerId);
    }
    public class ShoppingCartRepository : IShoppingCartRepository
    {

        private readonly IDapperDbConnection _dbContext;

        public ShoppingCartRepository(IDapperDbConnection dbContext) => _dbContext = dbContext;

        public async Task<bool> AddToCartAsync(Cart addToCartDTO)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();
            var cartId = 0;
            try
            {
                if (addToCartDTO.Id == 0)
                {
                    var sQuery = $"insert into Carts (CustomerId, IsCheckedOut, CreatedAt, UpdatedAt) values (@CustomerId, @IsCheckedOut, @CreatedAt, @UpdatedAt) SELECT CAST(SCOPE_IDENTITY() AS int)";
                    cartId = await dbConnection.QuerySingleAsync<int>(sQuery, new
                    {
                        addToCartDTO.CustomerId,
                        addToCartDTO.IsCheckedOut,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,

                    }, transaction);
                }
                else
                {
                    var sQuery = $"update Carts set UpdatedAt = @UpdatedAt where Id = @Id";

                    cartId = await dbConnection.ExecuteAsync(sQuery, new
                    {
                        addToCartDTO.Id,
                        UpdatedAt = DateTime.Now,

                    }, transaction);
                }

                foreach (var cartItem in addToCartDTO.CartItems)
                {
                    var cartItemInsertQuery = $"insert into CartItems values (@CartId, @ProductId, @Quantity, @UnitPrice, @Discount, @TotalPrice, @CreatedAt,@UpdatedAt)";
                    var result = await dbConnection.ExecuteAsync(cartItemInsertQuery, new
                    {
                        cartId,
                        cartItem.ProductId,
                        cartItem.Quantity,
                        cartItem.UnitPrice,
                        cartItem.Discount,
                        cartItem.TotalPrice,
                        cartItem.CreatedAt,
                        cartItem.UpdatedAt
                    }, transaction);
                }
                transaction.Commit();

                return true;
            }
            catch (Exception er)
            {
                transaction.Rollback();
                throw er;

            }

        }

        public async Task<CartResponseDTO> GetCartByCustomerIdAsync(int customerId)
        {

            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"  select c.Id, CustomerId , IsCheckedOut,c.CreatedAt, c.UpdatedAt, CartItemId = ci.Id, ProductId, p.Name, ProductImage = p.[Image1],Quantity, UnitPrice,  Discount, TotalPrice  from Carts c left join CartItems ci on ci.CartId = c.Id left join Products p on ci.ProductId = p.Id where c.CustomerId = @customerId and c.IsCheckedOut = 'false'";
            var cart = await dbConnection.QueryAsync<CartItemsQueryModel>(sQuery, new { customerId });

            return  cart.GroupBy(group => new
            {
                group.Id,
                group.CustomerId,
                group.IsCheckedOut,
                group.UpdatedAt,
                group.CreatedAt
            }).Select(c =>
            new CartResponseDTO()
            {
                Id = c.Key.Id,
                CustomerId = c.Key.CustomerId,
                IsCheckedOut = c.Key.IsCheckedOut,
                UpdatedAt = c.Key.UpdatedAt,
                CreatedAt = c.Key.CreatedAt,
                CartItems = c.Select(item =>
                new CartItemResponseDTO()
                {
                    Id = item.CartItemId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImage = item.ProductImage != null
                             ? $"data:image/png;base64,{Convert.ToBase64String(item.ProductImage)}"
                             : null,
                    TotalPrice = item.TotalPrice,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    Quantity = item.Quantity,
                }).ToList()
            }).FirstOrDefault();

        }

        public async Task<bool> UpdateCartItemAsync(CartItem updateCartDTO)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var cartItemInsertQuery = $"insert into CartItems values (@Quantity, @TotalPrice, @UpdatedAt)";
            var result = await dbConnection.ExecuteAsync(cartItemInsertQuery, new
            {
                updateCartDTO.Quantity,
                updateCartDTO.TotalPrice,
                updateCartDTO.UpdatedAt
            });
            return result > 0;
        }
    }

}


