using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace SportsStore.Pages
{
	public class OrderSummary
	{
		public Guid Id { get; set; }
		public DateTime CreatedAt { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; } = string.Empty;
		public List<OrderItemSummary> Items { get; set; } = new();
	}

	public class OrderItemSummary
	{
		public string ProductName { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}

	public class MyOrdersModel : PageModel
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public MyOrdersModel(IHttpClientFactory httpClientFactory)
			=> _httpClientFactory = httpClientFactory;

		[BindProperty(SupportsGet = true)]
		public string? CustomerId { get; set; }

		public List<OrderSummary>? Orders { get; set; }

		public async Task OnGetAsync()
		{
			if (!string.IsNullOrWhiteSpace(CustomerId))
			{
				try
				{
					var client = _httpClientFactory.CreateClient("OrderApi");
					Orders = await client.GetFromJsonAsync<List<OrderSummary>>($"api/customers/{CustomerId}/orders");
				}
				catch { Orders = new List<OrderSummary>(); }
			}
		}
	}
}
