using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;

namespace OrderManagementApi.Features.Orders
{
	public record GetOrderStatusQuery(Guid OrderId) : IRequest<string?>;

	public class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, string?>
	{
		private readonly OrderDbContext _context;

		public GetOrderStatusQueryHandler(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<string?> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

			return order?.Status.ToString();
		}
	}
}
