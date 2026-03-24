namespace OrderManagementApi.Models
{
	public enum OrderStatus
	{
		Submitted, InventoryPending, PaymentPending, ShippingPending,
		Completed, Failed, InventoryFailed, PaymentFailed
	}

	public class Order
	{
		public Guid Id { get; set; }
		public Guid CustomerId { get; set; }
		public OrderStatus Status { get; set; }
		public decimal TotalAmount { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? FailureReason { get; set; }
		public List<OrderItem> Items { get; set; } = new();
	}
}
