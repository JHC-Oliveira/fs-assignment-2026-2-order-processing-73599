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
				.WithOne(oi => oi.Order)
				.HasForeignKey(oi => oi.OrderId);

			modelBuilder.Entity<Order>()
				.Property(o => o.TotalAmount)
				.HasPrecision(18, 2);

			modelBuilder.Entity<OrderItem>()
				.HasKey(oi => oi.Id);

			modelBuilder.Entity<OrderItem>()
				.Property(oi => oi.Price)
				.HasPrecision(18, 2);
		}

	}
}