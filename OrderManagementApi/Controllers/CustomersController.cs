using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;

namespace OrderManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        public CustomersController(OrderDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
            => Ok(await _context.Customers.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetCustomerOrders(Guid id)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerId == id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            customer.Id = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
    }
}
