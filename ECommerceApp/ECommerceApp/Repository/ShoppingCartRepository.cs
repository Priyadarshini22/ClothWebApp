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
        Task<bool> AddToCartAsync(AddToCartDTO addToCartDTO);
        Task<bool> UpdateCartItemAsync(UpdateCartItemDTO updateCartDTO);
        Task<CartResponseDTO> GetCartByCustomerIdAsync(int customerId);
        Task<CartItemResponseDTO> GetCartItemById(int Id);
        Task<bool> UpdateCartAsync(int cartId);
        Task<bool> RemoveCartItem(RemoveCartItemDTO removeCartItemDTO);
    }
    public class ShoppingCartRepository : IShoppingCartRepository
    {

        private readonly IDapperDbConnection _dbContext;

        public ShoppingCartRepository(IDapperDbConnection dbContext) => _dbContext = dbContext;

        public async Task<bool> AddToCartAsync(AddToCartDTO addToCartDTO)
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
                        IsCheckedOut = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,

                    }, transaction);
                }
                else
                {
                    var sQuery = $"update Carts set UpdatedAt = @UpdatedAt where Id = @Id";
                    cartId = addToCartDTO.Id;
                    _ = await dbConnection.ExecuteAsync(sQuery, new
                    {
                        addToCartDTO.Id,
                        UpdatedAt = DateTime.Now,

                    }, transaction);
                }

                var cartItemInsertQuery = $"insert into CartItems values(@CartId,@ProductId,@Quantity,@UnitPrice,@Discount,@TotalPrice,@CreatedAt,@UpdatedAt, @SizeId)";
                var result = await dbConnection.ExecuteAsync(cartItemInsertQuery, new
                {
                    CartId = cartId,
                    addToCartDTO.ProductId,
                    addToCartDTO.Quantity,
                    addToCartDTO.UnitPrice,
                    addToCartDTO.Discount,
                    addToCartDTO.TotalPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    addToCartDTO.SizeId
                }, transaction);

                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;

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
            }).FirstOrDefault() ?? new CartResponseDTO();

        }

        public async Task<bool> UpdateCartItemAsync(UpdateCartItemDTO updateCartDTO)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var cartItemInsertQuery = $"update CartItems set Quantity = @Quantity, TotalPrice = @TotalPrice, UpdatedAt =@UpdatedAt where ID = @Id";
            var result = await dbConnection.ExecuteAsync(cartItemInsertQuery, new
            {
                updateCartDTO.Quantity,
                updateCartDTO.TotalPrice,
                UpdatedAt = DateTime.UtcNow,
                Id = updateCartDTO.CartItemId
            });
            return result > 0;
        }

        public async Task<bool> UpdateCartAsync(int cartId)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var cartItemInsertQuery = $"update Carts set IsCheckedOut =  @IsCheckedOut where ID = @cartId";
            var result = await dbConnection.ExecuteAsync(cartItemInsertQuery, new { cartId , IsCheckedOut = true});
            return result > 0;
        }



        public async Task<CartItemResponseDTO> GetCartItemById(int Id)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var cartItemfetchQuery = $"Select * CartItems where Id = @Id";
            var result = await dbConnection.QuerySingleAsync<CartItemResponseDTO>(cartItemfetchQuery, new
            {
                Id
            });
            return result;
        }

        public async Task<bool> RemoveCartItem(RemoveCartItemDTO removeCartItemDTO)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var cartItemfetchQuery = $"delete from CartItems where Id = @Id";
            var result = await dbConnection.ExecuteAsync(cartItemfetchQuery, new
            {
                Id = removeCartItemDTO.CartItemId
            });
            return result >=1;
        }
    }

}


