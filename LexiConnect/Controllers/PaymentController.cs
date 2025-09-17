using LexiConnect.Models.VnPay;
using LexiConnect.Services.VnPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiConnect.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost]
        public IActionResult CreatePayment(decimal amount, string orderDescription)
        {
            // 1. Build payment information model
            var paymentInfo = new PaymentInformationModel
            {
                Amount = amount,
                Name = "LexiConnect Subscription",
                OrderDescription = orderDescription,
                OrderType = "subscription"
            };

            // 2. Generate payment URL
            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);

            // 3. Redirect user to VNPAY page
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            // This will be called by VNPAY after payment is completed
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                // Payment success: handle subscription activation, save transaction to DB
                TempData["Success"] = "Payment successful!";
            }
            else
            {
                // Payment failed: handle accordingly
                TempData["Error"] = $"Payment failed, please try again";
            }

            return View("PaymentResult", response);
        }
    }
}
