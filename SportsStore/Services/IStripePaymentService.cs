namespace SportsStore.Services {

    public interface IStripePaymentService {
        Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd");
        Task<string> GetPaymentStatusAsync(string paymentIntentId);
    }
}