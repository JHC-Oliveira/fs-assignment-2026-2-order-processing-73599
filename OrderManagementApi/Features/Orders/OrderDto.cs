using MessageContracts;

namespace OrderManagementApi.Features.Orders
{
	public class OrderDto
	{
		public Guid Id { get; set; }
		public Guid CustomerId { get; set; }
		public string Status { get; set; } = string.Empty;
		public decimal TotalAmount { get; set; }
		public DateTime CreatedAt { get; set; }
		public List<OrderItemDto> Items { get; set; } = new();
	}
}
