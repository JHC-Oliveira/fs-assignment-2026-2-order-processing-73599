/*
 * OrderController.cs
 *
 * Handles the full checkout workflow including Stripe payment processing.
 * Flow:
 *   GET  Checkout       → Show shipping details form
 *   POST Checkout       → Validate shipping, create Stripe PaymentIntent, show payment form
 *   GET  PaymentResult  → Confirm payment status with Stripe, save or reject the order
 *   GET  PaymentFailed  → Shown when Stripe payment is declined or fails
 *   GET  PaymentCancelled → Shown when user cancels payment
 */

using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;
using SportsStore.Services;
using Stripe;

namespace SportsStore.Controllers {

    public class OrderController : Controller {
        private readonly IOrderRepository _repository;
        private readonly Cart _cart;
        private readonly IStripePaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderRepository repository,
            Cart cart,
            IStripePaymentService paymentService,
            IConfiguration config,
            ILogger<OrderController> logger) {
            _repository = repository;
            _cart = cart;
            _paymentService = paymentService;
            _config = config;
            _logger = logger;
        }

        // GET: Show shipping details form
        public ViewResult Checkout() => View(new Order());

        // POST: Validate shipping, create Stripe PaymentIntent, show payment form
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order) {
            if (_cart.Lines.Count() == 0) {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (!ModelState.IsValid) {
                _logger.LogWarning(
                    "Checkout validation failed for customer {CustomerName}",
                    order.Name);
                return View(order);
            }

            try {
                _logger.LogInformation(
                    "Initiating Stripe payment for customer {CustomerName}, Total: {OrderTotal:C}",
                    order.Name, _cart.ComputeTotalValue());

                // Create a Stripe PaymentIntent for the cart total
                var clientSecret = await _paymentService.CreatePaymentIntentAsync(
                    _cart.ComputeTotalValue());

                // Extract PaymentIntent ID from client secret (format: pi_xxx_secret_yyy)
                var paymentIntentId = clientSecret.Split("_secret_")[0];

                // Persist shipping details in TempData to restore after Stripe redirects back
                TempData["PaymentIntentId"] = paymentIntentId;
                TempData["CustomerName"] = order.Name;
                TempData["Line1"]        = order.Line1;
                TempData["Line2"]        = order.Line2;
                TempData["Line3"]        = order.Line3;
                TempData["City"]         = order.City;
                TempData["State"]        = order.State;
                TempData["Zip"]          = order.Zip;
                TempData["Country"]      = order.Country;
                TempData["GiftWrap"]     = order.GiftWrap.ToString();

                ViewBag.StripePublishableKey = _config["Stripe:PublishableKey"];
                ViewBag.ClientSecret         = clientSecret;
                ViewBag.OrderTotal           = _cart.ComputeTotalValue();

                return View("Payment", order);
            }
            catch (Exception ex) {
                _logger.LogError(ex,
                    "Failed to create Stripe PaymentIntent for customer {CustomerName}",
                    order.Name);
                ModelState.AddModelError("", "Payment setup failed. Please try again.");
                return View(order);
            }
        }

        // GET: Stripe redirects here after card entry — confirm the payment status
        public async Task<IActionResult> PaymentResult(string paymentIntentId) {
            if (string.IsNullOrEmpty(paymentIntentId)) {
                _logger.LogWarning("PaymentResult reached without a paymentIntentId");
                return RedirectToAction("PaymentFailed");
            }

            try {
                var status = await _paymentService.GetPaymentStatusAsync(paymentIntentId);

                _logger.LogInformation(
                    "PaymentResult received for {PaymentIntentId}, Status: {Status}",
                    paymentIntentId, status);

                if (status == "succeeded") {
                    // Rebuild the order from TempData
                    var order = new Order {
                        Name            = TempData["CustomerName"]?.ToString(),
                        Line1           = TempData["Line1"]?.ToString(),
                        Line2           = TempData["Line2"]?.ToString(),
                        Line3           = TempData["Line3"]?.ToString(),
                        City            = TempData["City"]?.ToString(),
                        State           = TempData["State"]?.ToString(),
                        Zip             = TempData["Zip"]?.ToString(),
                        Country         = TempData["Country"]?.ToString(),
                        GiftWrap        = bool.TryParse(TempData["GiftWrap"]?.ToString(), out var gw) && gw,
                        PaymentIntentId = paymentIntentId,
                        PaymentStatus   = status
                    };

                    order.Lines = _cart.Lines.ToArray();
                    _repository.SaveOrder(order);
                    _cart.Clear();

                    _logger.LogInformation(
                        "Order {OrderId} saved successfully for customer {CustomerName}, PaymentIntent: {PaymentIntentId}",
                        order.OrderID, order.Name, paymentIntentId);

                    return RedirectToPage("/Completed", new { orderId = order.OrderID });
                }

                // Payment incomplete or requires action
                _logger.LogWarning(
                    "Payment not completed for {PaymentIntentId}, Status: {Status}",
                    paymentIntentId, status);
                return RedirectToAction("PaymentFailed");
            }
            catch (Exception ex) {
                _logger.LogError(ex,
                    "Error confirming payment for {PaymentIntentId}",
                    paymentIntentId);
                return RedirectToAction("PaymentFailed");
            }
        }

        // GET: Shown when payment is declined or an error occurs
        public ViewResult PaymentFailed() => View();

        // GET: Shown when the user cancels the payment
        public ViewResult PaymentCancelled() => View();
    }
}
