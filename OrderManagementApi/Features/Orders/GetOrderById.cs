using MediatR;
using MessageContracts;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;

namespace OrderManagementApi.Features.Orders
{
	public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

	public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
	{
		private readonly OrderDbContext _context;

		public GetOrderByIdQueryHandler(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

			if (order == null) return null;

			return new OrderDto
			{
				Id = order.Id,
				CustomerId = order.CustomerId,
				Status = order.Status.ToString(),
				TotalAmount = order.TotalAmount,
				CreatedAt = order.CreatedAt,
				Items = order.Items.Select(i => new OrderItemDto(
					i.ProductId,
					i.ProductName,
					i.Quantity,
					i.Price
				)).ToList()
			};
		}
	}
}
