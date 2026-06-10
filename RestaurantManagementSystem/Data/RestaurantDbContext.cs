using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;
using Microsoft.EntityFrameworkCore.SqlServer;
namespace RestaurantManagementSystem.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }

        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure MenuItemIngredient (Many-to-Many join table mapping MenuItem to InventoryItem)
            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.MenuItem)
                .WithMany(m => m.Ingredients)
                .HasForeignKey(mi => mi.MenuItemId);

            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.InventoryItem)
                .WithMany()
                .HasForeignKey(mi => mi.InventoryItemId);

            // Configure OrderItem (Many-to-One mapping to Order)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Configure MenuItem relationship in OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId);

            // Configure Order relationship to Table
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Reservation relationship to Table
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany()
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
