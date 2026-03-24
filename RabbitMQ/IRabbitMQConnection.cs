using RabbitMQ.Client;

namespace RabbitMQ
{
	public interface IRabbitMQConnection : IDisposable
	{
		IChannel CreateChannelAsync();
	}
}
