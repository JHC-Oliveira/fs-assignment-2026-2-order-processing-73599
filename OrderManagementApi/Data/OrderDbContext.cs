using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data
{
	public class OrderDbContext : DbContext
	{
		public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Order>()
				.HasMany(o => o.Items)
				.WithOne() // Assuming OrderItem doesn't have a direct navigation property back to Order
				.HasForeignKey(oi => oi.OrderId);

			modelBuilder.Entity<OrderItem>()
				.HasKey(oi => oi.Id);

			// Seed data if necessary
			// modelBuilder.Entity<Product>().HasData(...);
		}
	}
}