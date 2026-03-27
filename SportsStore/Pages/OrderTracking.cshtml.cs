using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace SportsStore.Pages
{
	public class OrderDetail
	{
		public Guid Id { get; set; }
		public DateTime CreatedAt { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; } = string.Empty;
		public string? FailureReason { get; set; }
		public List<OrderItemSummary> Items { get; set; } = new();
	}

	public class OrderTrackingModel : PageModel
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public OrderTrackingModel(IHttpClientFactory httpClientFactory)
			=> _httpClientFactory = httpClientFactory;

		[BindProperty(SupportsGet = true)]
		public string? OrderId { get; set; }

		public OrderDetail? Order { get; set; }
		public bool Loading { get; set; }

		public async Task OnGetAsync()
		{
			if (!string.IsNullOrWhiteSpace(OrderId))
			{
				Loading = true;
				try
				{
					var client = _httpClientFactory.CreateClient("OrderApi");
					Order = await client.GetFromJsonAsync<OrderDetail>($"api/orders/{OrderId}");
				}
				catch { Order = null; }
				Loading = false;
			}
		}
	}
}
