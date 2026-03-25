using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;

namespace OrderManagementApi.Features.Orders
{
	public record GetOrdersQuery() : IRequest<List<OrderDto>>;

	public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderDto>>
	{
		private readonly OrderDbContext _context;
		private readonly IMapper _mapper;

		public GetOrdersQueryHandler(OrderDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
		{
			var orders = await _context.Orders
				.Include(o => o.Items)
				.OrderByDescending(o => o.CreatedAt)
				.ToListAsync(cancellationToken);

			return _mapper.Map<List<OrderDto>>(orders);
		}
	}
}
