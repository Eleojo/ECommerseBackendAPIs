using Core.ShoppingCartServices;
using Data.Dtos;
using Data.Model;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerseBackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;

        public CartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto request)
        {
            try
            {
                // Retrieve the userId from the claims
                var userIdClaim = User.FindFirst("customNameIdentifier");

                if (userIdClaim == null)
                {
                    return Unauthorized("UserId claim not found in token.");
                }

                // Convert the string userId from the claim to Guid
                if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return BadRequest("Invalid UserId format.");
                }

                // Call the service to add the item to the cart
                bool result = await _shoppingCartService.AddToCartAsync(request.ShoppingCartId, request.ProductId, request.Quantity);

                if (result)
                {
                    return Ok("Item added to cart successfully.");
                }
                else
                {
                    return BadRequest("Failed to add item to cart.");
                }
            }
            catch (Exception ex)
            {
                // Handle specific exceptions here, if needed
                return StatusCode(500, $"Error adding item to cart: {ex.Message}");
            }
        }


        [HttpDelete("remove-from-cart/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found in token.");
            }

            // Convert the string userId from the claim to Guid
            var userId = Guid.Parse(userIdClaim.Value);
            var shoppingCart = await _shoppingCartService.GetCartByUserIdAsync(userId);

            if (shoppingCart == null)
            {
                return BadRequest("Shopping cart not found");
            }

            await _shoppingCartService.RemoveFromCartAsync(shoppingCart.Id, cartItemId);
            return Ok("Item removed from cart");
        }

        [HttpGet("my-cart")]
        public async Task<IActionResult> GetCart()
        {
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found in token.");
            }

            // Convert the string userId from the claim to Guid
            var userId = Guid.Parse(userIdClaim.Value);
            var cart = await _shoppingCartService.GetCartByUserIdAsync(userId);

            if (cart != null)
            {
                return Ok(cart);
            }

            return NotFound("Cart not found");
        }
    }

}
