using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace RabbitMQ
{
	public interface IBusPublisher
	{
		Task Publish<T>(T message, string? routingKey = null) where T : class;
	}

	public class RabbitMQBus : IBusPublisher
	{
		private readonly IRabbitMQConnection _connection;
		private readonly ILogger<RabbitMQBus> _logger;

		public RabbitMQBus(IRabbitMQConnection connection, ILogger<RabbitMQBus> logger)
		{
			_connection = connection;
			_logger = logger;
		}

		public async Task Publish<T>(T message, string? routingKey = null) where T : class
		{
			var channel = _connection.CreateChannelAsync();

			await channel.ExchangeDeclareAsync(
				exchange: "order_exchange",
				type: ExchangeType.Topic,
				durable: true);

			var messageName = routingKey ?? typeof(T).Name;
			var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

			await channel.BasicPublishAsync(
				exchange: "order_exchange",
				routingKey: messageName,
				body: body);

			_logger.LogInformation("Published {MessageName} to RabbitMQ", messageName);
		}
	}
}
