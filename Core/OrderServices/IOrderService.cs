using Data.Dtos;
using Data.Enums;

namespace Core.OrderServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> PlaceOrderAsync(Guid userId, List<PlaceOrderDto> orderItems);
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);

        Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(Guid sellerId);
        Task<StatusChangeDto> UpdateOrderStatusAsync(Guid OrderId, OrderStatusEnum status);
    }
}