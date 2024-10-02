using Data.Dtos;
using Data.Enums;
using Data.Model;

namespace Core.OrderServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> PlaceOrderAsync(Guid userId, List<OrderItemDto> orderItems);
        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<Order> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(Guid sellerId);
        Task<bool> UpdateOrderStatusAsync(Guid OrderId, OrderStatusEnum status);
    }
}