using System.Threading.Channels;

namespace InventoryService
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;

		public Worker(ILogger<Worker> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken ct)
		{
			_channel.QueueBind("inventory_queue", "order_exchange", nameof(OrderSubmitted));
			var consumer = new AsyncEventingBasicConsumer(_channel);
			consumer.Received += async (m, ea) => {
				var evt = JsonSerializer.Deserialize<OrderSubmitted>(ea.Body.ToArray());
				// Logic: Check stock
				bool success = evt.Items.All(i => i.Quantity < 100); // Simulated logic
				await _publisher.Publish(new InventoryCheckCompleted(evt.OrderId, success, success ? null : "Out of stock"));
				_channel.BasicAck(ea.DeliveryTag, false);
			};
		}
	}
}
