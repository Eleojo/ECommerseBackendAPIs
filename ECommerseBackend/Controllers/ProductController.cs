using Core.ProductServices;
using Data.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerseBackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("create-product")]
        [Authorize(Policy = "RequireSellerRole")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            var sellerId = User.FindFirst("customNameIdentifier").Value;

            if (Guid.TryParse(sellerId, out var parsedSellerId))
            {
                var product = await _productService.CreateProductAsync(productDto, parsedSellerId);
                return Ok(product);
            }
            return BadRequest("Invalid seller ID.");
        }

        [HttpGet("get-products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("get-product-by-id")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }


        [HttpPut("update-product")]
        public async Task<IActionResult> UpdateProduct(Guid productId, ProductDto productDto)
        {
            try
            {
                var updatedProduct = await _productService.UpdateProduct(productId,productDto);
                return Ok("Product Updated Successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }


        
    }

}
