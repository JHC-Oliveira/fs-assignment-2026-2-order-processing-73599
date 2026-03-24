namespace OrderManagementApi.Models
{
	public class OrderItem
	{
		public int Id { get; set; } // Primary Key for OrderItem
		public int ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal Price { get; set; }

		// Foreign key to the Order
		public Guid OrderId { get; set; }
		public Order Order { get; set; } = default!; // Navigation property
	}
}
