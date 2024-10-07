using Data.Dtos;
using Data.Enums;
using Data.Model;

namespace Core.OrderServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> PlaceOrderAsync(Guid userId, List<PlaceOrderDto> orderItems);
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);

        Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(Guid sellerId);
        Task<bool> UpdateOrderStatusAsync(Guid OrderId, OrderStatusEnum status);
        Task<bool> UpdateOrderStatus(int orderId, OrderStatusEnum newStatus);
    }
}