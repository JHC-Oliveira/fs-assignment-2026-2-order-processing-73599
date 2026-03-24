using Microsoft.EntityFrameworkCore;

/*
 * EFOrderRepository.cs
 *
 * Repository implementation that persists Order entities using Entity Framework Core.
 * - Exposes a queryable `Orders` collection with related `Lines` and `Product` navigation properties.
 * - Implements `SaveOrder` to add or update orders and persist changes to the `StoreDbContext`.
 * - Uses structured logging (ILogger<T>) to record business events (order creation/update) and exceptions.
 *
 * Notes:
 * - Logging uses structured properties (not string concatenation) so entries are searchable by property.
 * - Exceptions are logged and re-thrown to allow higher-level handling.
 */

namespace SportsStore.Models
{

	public class EFOrderRepository : IOrderRepository
	{
		private StoreDbContext context;
		private readonly ILogger<EFOrderRepository> _logger;

		// Constructor injection - ASP.NET Core will provide the logger
		public EFOrderRepository(StoreDbContext ctx, ILogger<EFOrderRepository> logger)
		{
			context = ctx;
			_logger = logger;
		}

		public IQueryable<Order> Orders => context.Orders
							.Include(o => o.Lines)
							.ThenInclude(l => l.Product);

		public void SaveOrder(Order order)
		{
			try
			{
				context.AttachRange(order.Lines.Select(l => l.Product));

				if (order.OrderID == 0)
				{
					// New order creation
					context.Orders.Add(order);

					//STRUCTURED LOGGING
					_logger.LogInformation(
						"Creating new order for customer {CustomerName} with {ItemCount} items, Total: {OrderTotal:C}",
						order.Name,              // {CustomerName}
						order.Lines.Count,       // {ItemCount}
						order.Lines.Sum(l => l.Product.Price * l.Quantity)  // {OrderTotal}
					);
				}
				else
				{
					// Existing order update
					_logger.LogInformation(
						"Updating order {OrderId}, Shipped: {IsShipped}",
						order.OrderID,
						order.Shipped
					);
				}

				context.SaveChanges();

				if (order.OrderID == 0)
				{
					_logger.LogInformation("Order created successfully with ID {OrderId}", order.OrderID);
				}
			}
			catch (Exception ex)
			{
				//ERROR LOGGING
				_logger.LogError(ex, "Error saving order for customer {CustomerName}", order.Name);
				throw; // Re-throw to let calling code handle it
			}
		}
	}
}
