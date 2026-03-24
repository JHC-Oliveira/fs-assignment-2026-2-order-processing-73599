using MediatR;
using MessageContracts;
using OrderManagementApi.Data;
using OrderManagementApi.Models;
using RabbitMQ;

namespace OrderManagementApi.Features.Orders
{
	public record CheckoutOrderCommand(Guid CustomerId, List<OrderItemDto> Items) : IRequest<Guid>;

	public class CheckoutOrderHandler : IRequestHandler<CheckoutOrderCommand, Guid>
	{
		private readonly OrderDbContext _context;
		private readonly IBusPublisher _publisher;

		public CheckoutOrderHandler(OrderDbContext context, IBusPublisher publisher)
		{
			_context = context;
			_publisher = publisher;
		}

		public async Task<Guid> Handle(CheckoutOrderCommand request, CancellationToken ct)
		{
			var orderId = Guid.NewGuid();

			var order = new Order
			{
				Id = orderId,
				CustomerId = request.CustomerId,
				Status = OrderStatus.Submitted,
				TotalAmount = request.Items.Sum(x => x.Price * x.Quantity),
				CreatedAt = DateTime.UtcNow,
				Items = request.Items.Select(i => new OrderItem
				{
					ProductId = i.ProductId,
					ProductName = i.ProductName,
					Quantity = i.Quantity,
					Price = i.Price,
					OrderId = orderId  // ← uses orderId variable, not order
				}).ToList()
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync(ct);

			await _publisher.Publish(new OrderSubmitted(
				order.Id, order.CustomerId, request.Items, order.TotalAmount));

			return order.Id;
		}

	}
}
