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

        

        [HttpPut("update-order-status")]
        [Authorize(Policy = "RequireSellerRole")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatusEnum newStatus)
        {
            var userIdClaim = User.FindFirst("customNameIdentifier");
            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim not found.");
            }

            Guid sellerId = Guid.Parse(userIdClaim.Value);
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);

            if (result != null)
            {
                var oldStatus = result.OldStatus;
                var updatedStatus = result.NewStatus;
                return Ok(new
                {
                    message = $"Order status changed from {oldStatus} to {updatedStatus}",
                    //oldStatus = oldStatus,
                    //newStatus = updatedStatus
                });
            }

            return NotFound(new { message = "Order not found" });
        }


    }

}
