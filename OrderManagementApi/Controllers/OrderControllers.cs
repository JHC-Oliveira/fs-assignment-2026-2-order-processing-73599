using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Features.Orders;

namespace OrderManagementApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly IMediator _mediator;
		public OrdersController(IMediator mediator) => _mediator = mediator;

		[HttpPost("checkout")]
		public async Task<IActionResult> Checkout(CheckoutOrderCommand command)
		{
			var orderId = await _mediator.Send(command);
			return Ok(new { OrderId = orderId });
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetOrder(Guid id) => Ok(await _mediator.Send(new GetOrderByIdQuery(id)));

		[HttpGet]
		public async Task<IActionResult> GetOrders()
			=> Ok(await _mediator.Send(new GetOrdersQuery()));

		[HttpGet("{id}/status")]
		public async Task<IActionResult> GetOrderStatus(Guid id)
		{
			var status = await _mediator.Send(new GetOrderStatusQuery(id));
			if (status == null) return NotFound();
			return Ok(new { status });
		}
	}
}
