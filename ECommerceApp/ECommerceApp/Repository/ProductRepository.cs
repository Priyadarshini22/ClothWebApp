using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs.ProductDTOs;
using ECommerceApp.Models;

namespace ECommerceApp.Repository
{
    public interface IProductRepository
    {
        Task<bool> CheckProductNameExists(string Name);
        Task<bool> CreateNewProduct(Product product);
        Task<bool> UpdateProduct(ProductUpdateDTO product);
        Task<Product> GetProductById(int id);
        Task<bool> DeleteProduct(int Id);
        Task<List<Product>> GetAllProducts();
        Task<List<ProductResponseDTO>> GetProductsByCategoryId(int categoryId);
    }
    public class ProductRepository : IProductRepository
    {
        private readonly IDapperDbConnection _dbContext;

        public ProductRepository(IDapperDbConnection dbContext) => _dbContext = dbContext;

        public async Task<bool> CheckProductNameExists(string Name)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"select * from Products where Name = @Name";
            var result = await dbConnection.QueryAsync(sQuery, new { Name });

            return result.Count() >= 1 ? true : false;
        }

        public async Task<bool> CreateNewProduct(Product product)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();

            try
            {
                // Insert product
                var insertProductQuery = @"
            INSERT INTO dbo.Products 
                (Name, Description, Price, StockQuantity, DiscountPercentage, Image1, Image2, CategoryId, IsAvailable) 
            VALUES 
                (@Name, @Description, @Price, @StockQuantity, @DiscountPercentage, @Image1, @Image2, @CategoryId, @IsAvailable);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                var productId = await dbConnection.ExecuteScalarAsync<int>(
                    insertProductQuery,
                    new
                    {
                        product.Name,
                        product.Description,
                        product.Price,
                        product.StockQuantity,
                        product.DiscountPercentage,
                        product.Image1,
                        product.Image2,
                        product.CategoryId,
                        IsAvailable = true
                    },
                    transaction: transaction
                );

                // Insert each size stock
                if (product.ProductSizes != null && product.ProductSizes.Any())
                {
                    var insertSizeQuery = @"
                INSERT INTO ProductSizes (ProductId, Size, Quantity)
                VALUES (@ProductId, @Size, @Quantity);";

                    foreach (var size in product.ProductSizes)
                    {
                        await dbConnection.ExecuteAsync(
                            insertSizeQuery,
                            new
                            {
                                ProductId = productId,
                                Size = size.Size,
                                Quantity = size.Quantity
                            },
                            transaction: transaction
                        );
                    }
                }

                // Commit transaction
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("Error creating product: " + ex.Message);
                return false;
            }
        }


        public async Task<Product> GetProductById(int id)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = "select * from Products where ID = @id";
            var product = await dbConnection.QueryFirstAsync<Product>(sQuery, new { id });
            if (product != null)
            {
                var productSizeQuery = @"select * from ProductSizes where Productid = @productId";
                var productSizes = await dbConnection.QueryAsync<ProductSize>(productSizeQuery, new { productId = product.Id });
                product.ProductSizes = productSizes;
            }
            return product;
        }
        public async Task<bool> UpdateProduct(ProductUpdateDTO product)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"Update Products SET  Name = @Name, Description = @Description, Price @Price, StockQuantity =@StockQuantity, DiscountPercentage =@DiscountPercentage where ID = @Id";
            var result = await dbConnection.ExecuteAsync(sQuery, new { product.Name, product.Description, product.Price, product.StockQuantity, product.DiscountPercentage, product.Id });

            return result >= 1 ? true : false;
        }

        public async Task<bool> DeleteProduct(int Id)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"Delete from Products where ID = @Id";
            var result = await dbConnection.ExecuteAsync(sQuery, new { Id });

            return result >= 1 ? true : false;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"select * from Products";
            var result = await dbConnection.QueryAsync<Product>(sQuery);

            return result.ToList();
        }

        public async Task<List<ProductResponseDTO>> GetProductsByCategoryId(int categoryId)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"select * from Products where CategoryId = @categoryId";
            var result = await dbConnection.QueryAsync<ProductResponseDTO>(sQuery, new { categoryId});

            return result.ToList();
        }
    }
}
