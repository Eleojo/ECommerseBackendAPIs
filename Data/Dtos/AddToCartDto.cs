

namespace Data.Dtos
{
    public class AddToCartRequestDto
    {
        public Guid ShoppingCartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
