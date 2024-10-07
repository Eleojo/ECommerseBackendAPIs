using Core.OrderServices;
using Data.Dtos;
using Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerseBackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder( List<PlaceOrderDto> orderItems)
        {
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found in token.");
            }

            // Convert the string userId from the claim to Guid
            var userId = Guid.Parse(userIdClaim.Value);

            var result = await _orderService.PlaceOrderAsync(userId, orderItems);
            if (result != null)
            {
                return Ok("Order placed successfully");
            }

            return BadRequest("Failed to place the order.");
        }

        [HttpGet("user-orders")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found in token.");
            }

            // Convert the string userId from the claim to Guid
            var userId = Guid.Parse(userIdClaim.Value);

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            if (orders != null && orders.Any())
            {
                return Ok(orders);
            }

            return NotFound("No orders found for the user");
        }

        [HttpGet("get-order-by-id")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }

            return NotFound("Order not found");
        }

        [Authorize(Policy = "RequireSellerRole")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found.");
            }

            Guid sellerId = Guid.Parse(userIdClaim.Value);
            var orders = await _orderService.GetOrdersBySellerAsync(sellerId);
            return Ok(orders);
        }

        [Authorize(Policy ="RequireSellerRole")]
        [HttpPost("update-order-status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatusEnum status)
        {
            // Seller can only update their own order items
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found.");
            }

            Guid sellerId = Guid.Parse(userIdClaim.Value);

            var order = await _orderService.GetOrdersBySellerAsync(sellerId);
            if (!order.Any(o => o.Id == orderId))
            {
                return NotFound("Order not found for the seller.");
            }

            await _orderService.UpdateOrderStatusAsync(orderId, status);
            return Ok("Order status updated.");
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin, Seller")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderStatusEnum newStatus)
        {
            var updated = await _orderService.UpdateOrderStatus(orderId, newStatus);
            if (!updated)
            {
                return NotFound("Order not found");
            }

            return Ok("Order status updated");
        }


    }

}
