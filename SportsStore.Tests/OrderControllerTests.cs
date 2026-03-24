using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SportsStore.Controllers;
using SportsStore.Models;
using SportsStore.Services;
using Xunit;

namespace SportsStore.Tests {

    public class OrderControllerTests {

        // Helper: builds a controller with mocked dependencies including TempData
        private static OrderController BuildController(
            IOrderRepository repo,
            Cart cart,
            IStripePaymentService? paymentService = null) {

            var mockPayment = paymentService != null
                ? Mock.Get(paymentService)
                : new Mock<IStripePaymentService>();

            var mockConfig  = new Mock<IConfiguration>();
            var mockLogger  = new Mock<ILogger<OrderController>>();

            var controller = new OrderController(
                repo,
                cart,
                mockPayment.Object,
                mockConfig.Object,
                mockLogger.Object);

            // TempData must be provided — without it, TempData assignments throw NullReferenceException
            controller.TempData = new TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        [Fact]
        public async Task Cannot_Checkout_Empty_Cart() {
            // Arrange
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Cart cart   = new Cart();
            Order order = new Order();
            OrderController target = BuildController(mock.Object, cart);

            // Act
            ViewResult? result = await target.Checkout(order) as ViewResult;

            // Assert - order not saved
            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            // Assert - default view returned with invalid model state
            Assert.True(string.IsNullOrEmpty(result?.ViewName));
            Assert.False(result?.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Cannot_Checkout_Invalid_ShippingDetails() {
            // Arrange
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);
            OrderController target = BuildController(mock.Object, cart);
            target.ModelState.AddModelError("error", "error");

            // Act
            ViewResult? result = await target.Checkout(new Order()) as ViewResult;

            // Assert - order not saved
            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            // Assert - default view returned with invalid model state
            Assert.True(string.IsNullOrEmpty(result?.ViewName));
            Assert.False(result?.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Can_Checkout_Proceeds_To_Payment_View() {
            // Arrange - with Stripe, a successful POST now shows the Payment view
            // (order is only saved after PaymentResult confirms the charge)
            Mock<IOrderRepository> mockRepo     = new Mock<IOrderRepository>();
            Mock<IStripePaymentService> mockPay = new Mock<IStripePaymentService>();
            mockPay.Setup(s => s.CreatePaymentIntentAsync(It.IsAny<decimal>(), It.IsAny<string>()))
                   .ReturnsAsync("pi_test_secret_test");

            Cart cart = new Cart();
            cart.AddItem(new Product { ProductID = 1, Price = 10 }, 1);

            OrderController target = BuildController(mockRepo.Object, cart, mockPay.Object);

            // Act
            ViewResult? result = await target.Checkout(new Order {
                Name = "Test", Line1 = "1 St", City = "City", State = "ST", Country = "US"
            }) as ViewResult;

            // Assert - Payment view shown, order not yet saved
            Assert.Equal("Payment", result?.ViewName);
            mockRepo.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
        }
    }
}

