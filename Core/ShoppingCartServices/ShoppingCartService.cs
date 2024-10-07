using Data.AppDbContext;
using Data.Dtos;
using Data.Model;
using Microsoft.EntityFrameworkCore;


namespace Core.ShoppingCartServices
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ECommerceDbContext _context;

        public ShoppingCartService(ECommerceDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddToCartAsync(Guid userId, Guid productId, int quantity)
        {
            // Check if the product exists and has enough stock
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                throw new Exception("Product not available or insufficient stock.");
            }

            // Find the active cart for the user, or create a new one if none exists
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsOrdered)
                ?? await CreateCartAsync(userId); // Create new cart if none found

            // Check if the cart item already exists in the cart
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ShoppingCartId == cart.Id && ci.ProductId == productId);

            if (existingCartItem != null)
            {
                // Update the quantity of the existing cart item
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Add a new item to the cart
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    ShoppingCartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                await _context.CartItems.AddAsync(cartItem);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ShoppingCart> CreateCartAsync(Guid userId)
        {
            var newCart = new ShoppingCart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem>(),
                IsOrdered = false // Indicates that the cart is still active
            };

            await _context.ShoppingCarts.AddAsync(newCart);
            await _context.SaveChangesAsync();

            return newCart;
        }


        public async Task<bool> RemoveFromCartAsync(Guid shoppingCartId, Guid productId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ShoppingCartId == shoppingCartId && ci.ProductId == productId);

            if (cartItem != null)
            {
                // Remove the item from the cart completely
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return true;
            }

            return false; // Item not found
        }

        public async Task<ShoppingCartDto> GetCartByUserIdAsync(Guid userId)
        {
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (shoppingCart == null)
            {
                return null; // or throw an exception based on your application's needs
            }

            return new ShoppingCartDto
            {
                Id = shoppingCart.Id,
                UserId = shoppingCart.UserId,
                CartItems = shoppingCart.CartItems.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    ProductName = ci.Product.Name // Map necessary properties from the Product
                }).ToList()
            };
        }
        public async Task<bool> MarkCartAsOrderedAsync(Guid cartId)
        {
            var cart = await _context.ShoppingCarts.FindAsync(cartId);
            if (cart == null)
            {
                throw new Exception("Cart not found.");
            }

            cart.IsOrdered = true; // Mark the cart as completed
            await _context.SaveChangesAsync();

            return true;
        }


    }
}
