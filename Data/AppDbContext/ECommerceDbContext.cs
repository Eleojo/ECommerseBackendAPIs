using Data.Model;
using Microsoft.EntityFrameworkCore;


namespace Data.AppDbContext
{
    public class ECommerceDbContext : DbContext
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filters for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.User.IsDeleted);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => !oi.Product.IsDeleted);
            modelBuilder.Entity<ShoppingCart>().HasQueryFilter(sc => !sc.User.IsDeleted);
            modelBuilder.Entity<CartItem>().HasQueryFilter(ci => !ci.Product.IsDeleted);

            // Configure properties with specific types
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            });

            // Foreign key relationships with NoAction on delete to avoid cascade paths
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany() // If you have a collection of products in User, you may want to specify that
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.NoAction); // Change to NoAction or Restrict

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction); // Change to NoAction or Restrict

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.NoAction); // Change to NoAction or Restrict
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public override int SaveChanges()
        {
            HandleSoftDelete();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleSoftDelete()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
            {
                if (entry.Entity is ISoftDeletable entity)
                {
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                }
            }
        }
    }
}
