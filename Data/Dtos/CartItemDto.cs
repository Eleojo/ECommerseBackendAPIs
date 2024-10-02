
namespace Data.Dtos
{
    public class CartItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } // Include only necessary fields
    }
}
