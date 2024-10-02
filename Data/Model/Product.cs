

namespace Data.Model
{
    public class Product : ISoftDeletable
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public User Seller { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stock {  get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool IsDeleted { get; set; }

    }
}
