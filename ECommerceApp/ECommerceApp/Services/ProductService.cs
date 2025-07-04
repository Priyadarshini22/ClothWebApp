using Microsoft.EntityFrameworkCore;
using ECommerceApp.Data;
using ECommerceApp.DTOs.ProductDTOs;
using ECommerceApp.Models;
using ECommerceApp.DTOs;
using ECommerceApp.Repository;

namespace ECommerceApp.Services
{
    public class ProductService
    {
        private readonly IDapperDbConnection _context;
        private readonly IProductRepository _productRepository;

        public ProductService(IDapperDbConnection context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<ApiResponse<bool>> CreateProductAsync(Product productDto)
        {
            try
            {
                // Check if product name already exists (case-insensitive)
                if (await _productRepository.CheckProductNameExists(productDto.Name))
                {
                    return new ApiResponse<bool>(400, "Product name already exists.");  
                }

                var result = await _productRepository.CreateNewProduct(productDto);

                return new ApiResponse<bool>(200, result);
            }
            catch (Exception ex)
            {
                // Log the exception (implementation depends on your logging setup)
                return new ApiResponse<bool>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductResponseDTO>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetProductById(id);

                if (product == null)
                {
                    return new ApiResponse<ProductResponseDTO>(404, "Product not found.");
                }

                var productResponse = new ProductResponseDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    DiscountPercentage = product.DiscountPercentage,
                    CategoryId = product.CategoryId,
                    IsAvailable = product.IsAvailable,
                    Image1 = product.Image1 != null
                             ? $"data:image/png;base64,{Convert.ToBase64String(product.Image1)}"
                             : null,
                    Image2 = product.Image2 != null
                             ? $"data:image/png;base64,{Convert.ToBase64String(product.Image2)}"
                             : null,
                    ProductSizes = product.ProductSizes,
                };

                return new ApiResponse<ProductResponseDTO>(200, productResponse);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateProductAsync(ProductUpdateDTO productDto)
        {
            try
            {
                var product = await _productRepository.UpdateProduct(productDto);
                //if (product == null)
                //{
                //    return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
                //}

                //// Check if the new product name already exists (case-insensitive), excluding the current product
                //if (await _context.Products.AnyAsync(p => p.Name.ToLower() == productDto.Name.ToLower() && p.Id != productDto.Id))
                //{
                //    return new ApiResponse<ConfirmationResponseDTO>(400, "Another product with the same name already exists.");
                //}

                //// Check if Category exists
                //if (!await _context.Categories.AnyAsync(cat => cat.Id == productDto.CategoryId))
                //{
                //    return new ApiResponse<ConfirmationResponseDTO>(400, "Specified category does not exist.");
                //}

                //// Update product properties manually
                //product.Name = productDto.Name;
                //product.Description = productDto.Description;
                //product.Price = productDto.Price;
                //product.StockQuantity = productDto.StockQuantity;
                //product.DiscountPercentage = productDto.DiscountPercentage;
                //product.CategoryId = productDto.CategoryId;
                //product.Image1 = productDto.Image1;
                //product.Image2 = productDto.Image2;


                //await _context.SaveChangesAsync();

                // Prepare confirmation message
                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Product with Id {productDto.Id} updated successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _productRepository.DeleteProduct(id);

                if (product == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
                }

                // Implementing Soft Delete
                //product.IsAvailable = false;
                //await _context.SaveChangesAsync();

                // Prepare confirmation message
                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Product with Id {id} deleted successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();

                var productList = products.Select(p => new ProductResponseDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    DiscountPercentage = p.DiscountPercentage,
                    CategoryId = p.CategoryId,
                    IsAvailable = p.IsAvailable,
                    Image1 = p.Image1 != null
                             ? $"data:image/png;base64,{Convert.ToBase64String(p.Image1)}"
                             : null,
                    Image2 = p.Image2 != null
                             ? $"data:image/png;base64,{Convert.ToBase64String(p.Image2)}"
                             : null,
                }).ToList();

                return new ApiResponse<List<ProductResponseDTO>>(200, productList);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<List<ProductResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetAllProductsByCategoryAsync(int categoryId)
        {
            try
            {
                // Retrieve products associated with the specified category
                var products = await _productRepository.GetProductsByCategoryId(categoryId);

                if (products == null || products.Count == 0)
                {
                    return new ApiResponse<List<ProductResponseDTO>>(404, "Products not found.");
                }

                var productList = products.Select(p => new ProductResponseDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    DiscountPercentage = p.DiscountPercentage,
                    CategoryId = p.CategoryId,
                    IsAvailable = p.IsAvailable,
                    Image1 = p.Image1,
                    Image2 = p.Image2,
                }).ToList();

                return new ApiResponse<List<ProductResponseDTO>>(200, productList);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<List<ProductResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        //public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateProductStatusAsync(ProductStatusUpdateDTO productStatusUpdateDTO)
        //{
        //    try
        //    {
        //        var product = await _context.Products
        //            .FirstOrDefaultAsync(p => p.Id == productStatusUpdateDTO.ProductId);

        //        if (product == null)
        //        {
        //            return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
        //        }

        //        product.IsAvailable = productStatusUpdateDTO.IsAvailable;
        //        await _context.SaveChangesAsync();

        //        // Prepare confirmation message
        //        var confirmationMessage = new ConfirmationResponseDTO
        //        {
        //            Message = $"Product with Id {productStatusUpdateDTO.ProductId} Status Updated successfully."
        //        };

        //        return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
        //    }
        //}
    }
}