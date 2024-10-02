using Core.ProductServices;
using Data.AppDbContext;
using Data.Dtos;
using Data.Enums;
using Data.Model;
using Microsoft.EntityFrameworkCore;


namespace Core.OrderServices
{
    public class OrderService : IOrderService
    {
        private readonly ECommerceDbContext _context;
        private readonly IProductService _productService;

        public OrderService(ECommerceDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<OrderResponseDto> PlaceOrderAsync(Guid userId, List<OrderItemDto> orderItems)
        {
            if (orderItems == null || !orderItems.Any())
            {
                throw new ArgumentException("Order items cannot be null or empty.");
            }

            // Validate products and calculate total amount
            decimal totalAmount = 0;
            var orderItemsWithPrices = new List<OrderItem>();

            // Start a new transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in orderItems)
                {
                    var product = await _productService.GetProductBiId(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                    {
                        throw new Exception($"Product {item.ProductId} is not available or has insufficient stock.");
                    }

                    totalAmount += product.Price * item.Quantity;

                    orderItemsWithPrices.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });

                    // Update product stock
                    product.Stock -= item.Quantity;
                }

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    OrderItems = orderItemsWithPrices
                };

                _context.Orders.Add(order);

                // Save all changes
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                return new OrderResponseDto
                {
                    OrderId = order.Id,
                    TotalAmount = totalAmount,
                    OrderDate = order.OrderDate,
                };
            }
            catch (Exception ex)
            {
                // Roll back the transaction if anything goes wrong
                await transaction.RollbackAsync();
                // Optionally log the exception
                throw new Exception("An error occurred while placing the order.", ex);
            }
        }


        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(Guid sellerId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            });
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid OrderId, OrderStatusEnum status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == OrderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
