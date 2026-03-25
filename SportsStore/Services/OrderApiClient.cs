using System.Text;
using System.Text.Json;

namespace SportsStore.Services
{
	public interface IOrderApiClient
	{
		Task<Guid?> CheckoutAsync(Guid customerId, List<OrderApiItem> items);
	}

	public record OrderApiItem(int ProductId, string ProductName, int Quantity, decimal Price);

	public class OrderApiClient : IOrderApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<OrderApiClient> _logger;

		public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<Guid?> CheckoutAsync(Guid customerId, List<OrderApiItem> items)
		{
			var payload = new { customerId, items };
			var json = JsonSerializer.Serialize(payload);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			try
			{
				var response = await _httpClient.PostAsync("/api/orders/checkout", content);
				response.EnsureSuccessStatusCode();

				var result = await response.Content.ReadAsStringAsync();
				var doc = JsonDocument.Parse(result);
				var orderIdStr = doc.RootElement.GetProperty("orderId").GetString();
				return Guid.TryParse(orderIdStr, out var orderId) ? orderId : null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to call OrderManagementApi checkout");
				return null;
			}
		}
	}
}
