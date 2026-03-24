using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RabbitMQ
{
	public interface IBusPublisher { Task Publish<T>(T message); }

	public class RabbitMQBus : IBusPublisher
	{
		private readonly IModel _channel;
		public RabbitMQBus(IConnection connection) => _channel = connection.CreateModel();

		public Task Publish<T>(T message)
		{
			var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
			_channel.BasicPublish(exchange: "order_exchange", routingKey: typeof(T).Name, body: body);
			return Task.CompletedTask;
		}
	}

}
