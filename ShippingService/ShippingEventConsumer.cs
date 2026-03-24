using MessageContracts;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ShippingService
{
	public class ShippingEventConsumer : BackgroundService
	{
		private readonly IChannel _channel;
		private readonly IBusPublisher _publisher;
		private readonly ILogger<ShippingEventConsumer> _logger;

		public ShippingEventConsumer(
			IRabbitMQConnection connection,
			IBusPublisher publisher,
			ILogger<ShippingEventConsumer> logger)
		{
			_channel = connection.CreateChannelAsync();
			_publisher = publisher;
			_logger = logger;

			_channel.ExchangeDeclareAsync("order_exchange", ExchangeType.Topic, durable: true)
				.GetAwaiter().GetResult();
			_channel.QueueDeclareAsync("shipping_queue", durable: true, exclusive: false, autoDelete: false)
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("shipping_queue", "order_exchange", nameof(PaymentProcessed))
				.GetAwaiter().GetResult();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var consumer = new AsyncEventingBasicConsumer(_channel);

			consumer.ReceivedAsync += async (model, ea) =>
			{
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());
				var paymentResult = JsonSerializer.Deserialize<PaymentProcessed>(message)!;

				// Only ship if payment succeeded
				if (paymentResult.IsSuccess)
				{
					var trackingNumber = Guid.NewGuid().ToString()[..8].ToUpper();
					var estimatedDelivery = DateTime.UtcNow.AddDays(3);

					_logger.LogInformation("Shipment created for Order {OrderId}. Tracking: {TrackingNumber}",
						paymentResult.OrderId, trackingNumber);

					await _publisher.Publish(new ShippingCreated(
						paymentResult.OrderId, trackingNumber, estimatedDelivery));
				}
				else
				{
					_logger.LogInformation("Skipping shipment for Order {OrderId} — payment failed",
						paymentResult.OrderId);
				}

				await _channel.BasicAckAsync(ea.DeliveryTag, false);
			};

			_channel.BasicConsumeAsync("shipping_queue", autoAck: false, consumer: consumer)
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
