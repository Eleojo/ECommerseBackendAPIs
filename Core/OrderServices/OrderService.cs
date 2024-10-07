using Core.ProductServices;
using Core.ShoppingCartServices;
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
        private readonly IShoppingCartService _shoppingCartService;

        public OrderService(ECommerceDbContext context, IProductService productService, IShoppingCartService shoppingCartService)
        {
            _context = context;
            _productService = productService;
            this._shoppingCartService = shoppingCartService;
        }

        public async Task<OrderResponseDto> PlaceOrderAsync(Guid userId, List<PlaceOrderDto> orderItems)
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
                    _context.Products.Update(product);
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

                // Mark cart as ordered
                var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsOrdered);
                await _shoppingCartService.MarkCartAsOrderedAsync(cart.Id);

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


        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
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
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.Product.Name,
                }).ToList(),


            });
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId)
        {
            // Fetch the order and related data
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            // Check if the order exists
            if (order == null)
            {
                return null; // Handle this case as you need (e.g., throw exception or return NotFound)
            }

            // Map the Order entity to the OrderDto
            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,  // Assuming Product has a 'Name' property
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return orderDto;
        }


        public async Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(Guid sellerId)
        {
            try
            {
                // Fetch orders where the seller has products in the order items
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                    .ToListAsync();

                // Return the orders as DTOs
                return orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        ProductName = oi.Product.Name,
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                throw new ApplicationException($"An error occurred while fetching orders for seller: {ex.Message}", ex);
            }
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

        public async Task<bool> UpdateOrderStatus(int orderId, OrderStatusEnum newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;  // Order not found
            }

            // Update status
            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        // Method to get order status for a user
        public async Task<OrderStatusEnum> GetOrderStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            return order?.Status ?? OrderStatusEnum.Pending;
        }
    }
}
