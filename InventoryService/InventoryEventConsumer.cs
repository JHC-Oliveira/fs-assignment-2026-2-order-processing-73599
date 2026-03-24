using MessageContracts;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace InventoryService
{
	public class InventoryEventConsumer : BackgroundService
	{
		private readonly IChannel _channel;
		private readonly IBusPublisher _publisher;
		private readonly ILogger<InventoryEventConsumer> _logger;

		// Simulated stock: productId → quantity available
		private readonly Dictionary<int, int> _stock = new()
		{
			{ 1, 100 },
			{ 2, 50 },
			{ 3, 10 }
		};

		public InventoryEventConsumer(
			IRabbitMQConnection connection,
			IBusPublisher publisher,
			ILogger<InventoryEventConsumer> logger)
		{
			_channel = connection.CreateChannelAsync();
			_publisher = publisher;
			_logger = logger;

			_channel.ExchangeDeclareAsync("order_exchange", ExchangeType.Topic, durable: true)
				.GetAwaiter().GetResult();
			_channel.QueueDeclareAsync("inventory_queue", durable: true, exclusive: false, autoDelete: false)
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("inventory_queue", "order_exchange", nameof(OrderSubmitted))
				.GetAwaiter().GetResult();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var consumer = new AsyncEventingBasicConsumer(_channel);

			consumer.ReceivedAsync += async (model, ea) =>
			{
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());
				var order = JsonSerializer.Deserialize<OrderSubmitted>(message)!;

				bool isSuccess = true;
				string? reason = null;

				foreach (var item in order.Items)
				{
					if (!_stock.TryGetValue(item.ProductId, out var stock) || stock < item.Quantity)
					{
						isSuccess = false;
						reason = $"Insufficient stock for product {item.ProductName}";
						break;
					}
				}

				if (isSuccess)
				{
					foreach (var item in order.Items)
						_stock[item.ProductId] -= item.Quantity;

					_logger.LogInformation("Inventory confirmed for Order {OrderId}", order.OrderId);
				}
				else
				{
					_logger.LogWarning("Inventory failed for Order {OrderId}: {Reason}", order.OrderId, reason);
				}

				await _publisher.Publish(new InventoryCheckCompleted(order.OrderId, isSuccess, reason));
				await _channel.BasicAckAsync(ea.DeliveryTag, false);
			};

			_channel.BasicConsumeAsync("inventory_queue", autoAck: false, consumer: consumer)
				.GetAwaiter().GetResult();

			return Task.CompletedTask;
		}

		public override void Dispose()
		{
			_channel.CloseAsync().GetAwaiter().GetResult();
			_channel.Dispose();
			base.Dispose();
		}
	}
}
