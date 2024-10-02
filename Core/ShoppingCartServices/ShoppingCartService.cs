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
        public async Task<ShoppingCart> CreateCartAsync(Guid userId)
        {
            var existingCart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (existingCart != null)
            {
                return existingCart; // Return existing cart if found
            }

            var newCart = new ShoppingCart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem>()
            };

            await _context.ShoppingCarts.AddAsync(newCart);
            await _context.SaveChangesAsync();

            return newCart;
        }


        public async Task<bool> AddToCartAsync(Guid shoppingCartId, Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                throw new Exception("Product not available or insufficient stock.");
                return false;
            }

            // Check if the cart item already exists
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ShoppingCartId == shoppingCartId && ci.ProductId == productId);

            if (existingCartItem != null)
            {
                // Update the quantity of the existing item
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Add a new item to the cart
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    ShoppingCartId = shoppingCartId,
                    ProductId = productId,
                    Quantity = quantity
                };

                await _context.CartItems.AddAsync(cartItem);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            return true;
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

    }
}
