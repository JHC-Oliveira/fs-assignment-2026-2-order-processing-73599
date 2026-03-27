using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Features.Orders;
using OrderManagementApi.Models;

namespace OrderManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly OrderDbContext _context;

        public OrdersController(IMediator mediator, OrderDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutOrderCommand command)
        {
            var orderId = await _mediator.Send(command);
            return Ok(new { OrderId = orderId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
            => Ok(await _mediator.Send(new GetOrderByIdQuery(id)));

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

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return Ok(orders);
        }
    }
}
