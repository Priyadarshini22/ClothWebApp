using ECommerceApp.DTOs;
using ECommerceApp.DTOs.ProductDTOs;
using ECommerceApp.Models;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ECommerceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        // Injecting the ProductService
        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // Creates a new product.
        [HttpPost("CreateProduct")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> CreateProduct([FromForm] ProductCreateDTO productDto,[FromForm] string StockBySize)
        {
            byte[]? image1Bytes = null;
            byte[]? image2Bytes = null;

            if (productDto.Images != null && productDto.Images.Count > 0)
            {
                using var ms1 = new MemoryStream();
                await productDto.Images[0].CopyToAsync(ms1);
                image1Bytes = ms1.ToArray();

                if (productDto.Images.Count > 1)
                {
                    using var ms2 = new MemoryStream();
                    await productDto.Images[1].CopyToAsync(ms2);
                    image2Bytes = ms2.ToArray();
                }
            }

            // Create a custom DTO for your service if needed
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                DiscountPercentage = productDto.DiscountPercentage,
                CategoryId = productDto.CategoryId,
                Image1 = image1Bytes,
                Image2 = image2Bytes,
                ProductSizes = JsonSerializer.Deserialize<Dictionary<string, int>>(StockBySize).Select(s => new ProductSize
                {
                    Size = s.Key,
                    Quantity = s.Value
                }).ToList()
            };

            var result = await _productService.CreateProductAsync(product);
            return Ok(result);
        }

        // Retrieves a product by ID.
        [HttpGet("GetProductById/{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> GetProductById(int id)
        {

            var response = await _productService.GetProductByIdAsync(id);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Updates an existing product.
        [HttpPut("UpdateProduct")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> UpdateProduct([FromBody] ProductUpdateDTO productDto)
        {
            var response = await _productService.UpdateProductAsync(productDto);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Deletes a product by ID.
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> DeleteProduct(int id)
        {
            var response = await _productService.DeleteProductAsync(id);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves all products.
        [HttpGet("GetAllProducts")]
        public async Task<ActionResult<ApiResponse<List<ProductResponseDTO>>>> GetAllProducts()
        {
            var response = await _productService.GetAllProductsAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves all products by category.
        [HttpGet("GetAllProductsByCategory/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<ProductResponseDTO>>>> GetAllProductsByCategory(int categoryId)
        {
            var response = await _productService.GetAllProductsByCategoryAsync(categoryId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Update Product Status
        //[HttpPut("UpdateProductStatus")]
        //public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> UpdateProductStatus(ProductStatusUpdateDTO productStatusUpdateDTO)
        //{
        //    var response = await _productService.UpdateProductStatusAsync(productStatusUpdateDTO);
        //    if (response.StatusCode != 200)
        //    {
        //        return StatusCode(response.StatusCode, response);
        //    }
        //    return Ok(response);
        //}
    }
}