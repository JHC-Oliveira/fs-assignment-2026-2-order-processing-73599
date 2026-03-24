/*
 * StripePaymentService.cs
 *
 * Clean service layer for all Stripe API interactions.
 * - Creates PaymentIntents to initiate a charge.
 * - Retrieves PaymentIntent status to confirm payment outcome.
 * - API keys are injected via IConfiguration (sourced from user secrets, never appsettings.json).
 * - All Stripe calls are wrapped in try/catch with structured logging.
 */

using Stripe;

namespace SportsStore.Services {

    public class StripePaymentService : IStripePaymentService {
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration config, ILogger<StripePaymentService> logger) {
            _logger = logger;
            // Set the secret key from user secrets / environment variables
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        }

        public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd") {
            try {
                // Stripe amounts are in the smallest currency unit (cents for USD)
                var options = new PaymentIntentCreateOptions {
                    Amount = (long)(amount * 100),
                    Currency = currency,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions {
                        Enabled = true
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe PaymentIntent created: {PaymentIntentId} for amount {Amount} {Currency}",
                    intent.Id, amount, currency.ToUpper());

                return intent.ClientSecret;
            }
            catch (StripeException ex) {
                _logger.LogError(ex,
                    "Stripe error creating PaymentIntent for amount {Amount}: {StripeErrorMessage}",
                    amount, ex.StripeError?.Message);
                throw;
            }
        }

        public async Task<string> GetPaymentStatusAsync(string paymentIntentId) {
            try {
                var service = new PaymentIntentService();
                var intent = await service.GetAsync(paymentIntentId);

                _logger.LogInformation(
                    "Stripe PaymentIntent {PaymentIntentId} status: {Status}",
                    paymentIntentId, intent.Status);

                return intent.Status;
            }
            catch (StripeException ex) {
                _logger.LogError(ex,
                    "Stripe error retrieving PaymentIntent {PaymentIntentId}: {StripeErrorMessage}",
                    paymentIntentId, ex.StripeError?.Message);
                throw;
            }
        }
    }
}