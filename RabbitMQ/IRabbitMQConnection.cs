using RabbitMQ.Client;

namespace RabbitMQ
{
	public interface IRabbitMQConnection : IDisposable
	{
		IModel CreateModel();
	}
}
