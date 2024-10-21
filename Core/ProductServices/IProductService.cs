using Data.Dtos;
using Data.Model;

namespace Core.ProductServices
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductDto productDto, Guid sellerId);
        Task<Product> GetProductBiId(Guid productId);
        Task<List<Product>> GetProductsAsync();
        Task<Product> UpdateProduct(Guid ProductId, ProductDto productDto);
    }
}