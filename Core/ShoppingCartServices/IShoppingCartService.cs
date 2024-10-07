using Data.Dtos;
using Data.Model;

namespace Core.ShoppingCartServices
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart> CreateCartAsync(Guid userId);
        Task<bool> AddToCartAsync(Guid userId,Guid productId, int quantity);
        Task<ShoppingCartDto> GetCartByUserIdAsync(Guid userId);
        Task<bool> RemoveFromCartAsync(Guid shoppingCartId, Guid productId);
        Task<bool> MarkCartAsOrderedAsync(Guid cartId);
    }
}