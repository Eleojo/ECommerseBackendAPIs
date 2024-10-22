using Data.AppDbContext;
using Data.Dtos;
using Data.Enums;
using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace Core.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly ECommerceDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "productList";

        public ProductService(ECommerceDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Product> GetProductById(Guid productId)
        {
            // Get the cached product list (or null if not cached)
            var products = await GetCachedProductsAsync();

            // If products are cached, try to find the product in the cache
            if (products != null)
            {
                var cachedProduct = products.FirstOrDefault(p => p.Id == productId);
                if (cachedProduct != null)
                {
                    return cachedProduct;
                }
            }

            // If product is not in the cache, fetch it from the database
            var dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (dbProduct != null)
            {
                // If no cached list exists, initialize it
                if (products == null)
                {
                    products = new List<Product>();
                }

                // Add the product to the list and update the cache
                products.Add(dbProduct);
                await SetCachedProductsAsync(products);

                return dbProduct;
            }

            return null; // If product is not found in the database
        }


        public async Task<List<Product>> GetProductsAsync()
        {
            // Try to get the cached products
            var products = await GetCachedProductsAsync();

            // If cache is empty, fetch products from the database
            if (products == null)
            {
                products = await _context.Products.ToListAsync();

                // Set the products in the cache
                await SetCachedProductsAsync(products);
            }

            return products;
        }

        public async Task<Product> CreateProductAsync(ProductDto productDto, Guid sellerId)
        {
            var seller = await _context.Users.FindAsync(sellerId);
            if(seller == null)
            {
                throw new Exception("Seller does not exist");
            }
            if (seller.Role != UserRoleEnum.Seller)
            {
                throw new Exception ( "Invalid Seller" );
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                Name = productDto.Name,
                Description = productDto.Description,
                Category = productDto.Category,
                Stock = productDto.Stock,
                Price = productDto.Price,
                IsDeleted = false
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;

        }

        public async Task<Product> UpdateProduct(Guid ProductId, ProductDto productDto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == ProductId);

            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {ProductId} not found.");
            }

            // Update the product's properties
            product.Name = productDto.Name; 
            product.Description = productDto.Description; 
            product.Price = productDto.Price; 
            product.Category = productDto.Category;
            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated product
            return product;
        }

        private async Task<List<Product>> GetCachedProductsAsync()
        {
            var cachedProductsJson = await _cache.GetStringAsync(CacheKey);

            if (!string.IsNullOrEmpty(cachedProductsJson))
            {
                Console.WriteLine("Cache Hit: Returning products from Redis.");
                return JsonSerializer.Deserialize<List<Product>>(cachedProductsJson);
            }
            else
            {
                Console.WriteLine("Cache Miss: No products found in Redis.");
                return null;
            }
        }

        private async Task SetCachedProductsAsync(List<Product> products)
        {
            var serializedProducts = JsonSerializer.Serialize(products);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            Console.WriteLine("Setting products in Redis Cache.");
            await _cache.SetStringAsync(CacheKey, serializedProducts, cacheOptions);
        }



    }
}
