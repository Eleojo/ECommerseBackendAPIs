﻿using Data.AppDbContext;
using Data.Dtos;
using Data.Enums;
using Data.Model;
using Microsoft.EntityFrameworkCore;


namespace Core.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly ECommerceDbContext _context;

        public ProductService(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductBiId(Guid productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            { return null; }
            return product;
        }

        public async Task<List<Product>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            if (products == null)
            {
                return null;
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

    }
}