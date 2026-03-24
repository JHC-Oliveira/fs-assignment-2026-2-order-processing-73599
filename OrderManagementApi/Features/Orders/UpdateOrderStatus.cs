using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;

namespace OrderManagementApi.Features.Orders
{
	public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus, string? Reason = null) : IRequest<bool>;

	public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
	{
		private readonly OrderDbContext _context;

		public UpdateOrderStatusCommandHandler(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

			if (order == null) return false;

			order.Status = request.NewStatus;

			await _context.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}
