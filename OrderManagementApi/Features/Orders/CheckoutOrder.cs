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
		private readonly IBusPublisher _publisher; // Custom RabbitMQ wrapper

		public CheckoutOrderHandler(OrderDbContext context, IBusPublisher publisher)
		{
			_context = context;
			_publisher = publisher;
		}

		public async Task<Guid> Handle(CheckoutOrderCommand request, CancellationToken ct)
		{
			var order = new Order
			{
				Id = Guid.NewGuid(),
				CustomerId = request.CustomerId,
				Status = OrderStatus.Submitted,
				TotalAmount = request.Items.Sum(x => x.Price * x.Quantity),
				CreatedAt = DateTime.UtcNow,
				Items = request.Items.Select(i => new OrderItem { /* mapping */ }).ToList()
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync(ct);

			// Publish to RabbitMQ
			await _publisher.Publish(new OrderSubmitted(order.Id, order.CustomerId, request.Items, order.TotalAmount));

			return order.Id;
		}
	}
}
