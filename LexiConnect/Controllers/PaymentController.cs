using BusinessObjects;
using LexiConnect.Models.VnPay;
using LexiConnect.Services.VnPay;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IGenericService<PaymentRecord> _paymentRecordService;
        private readonly IGenericService<SubscriptionPlan> _subscriptionPlanService;
        private readonly IGenericService<Users> _userService;

        public PaymentController(IVnPayService vnPayService, IGenericService<PaymentRecord> paymentRecordService, IGenericService<SubscriptionPlan> subscriptionPlanService, IGenericService<Users> userService)
        {
            _vnPayService = vnPayService;
            _paymentRecordService = paymentRecordService;
            _subscriptionPlanService = subscriptionPlanService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                var txnRef = Request.Query["vnp_TxnRef"].ToString();
                var description = Request.Query["vnp_OrderInfo"].ToString();
                var orderType = HttpContext.Session.GetString($"OrderType_{txnRef}");
                var subscriptionId = HttpContext.Session.GetString($"SubscriptionId_{txnRef}");
                var currency = HttpContext.Session.GetString($"Currency_{txnRef}");
                var durationMonths = HttpContext.Session.GetString($"DurationMonths_{txnRef}");

                var amount = decimal.Parse(Request.Query["vnp_Amount"].ToString()) / 100;

                if (!int.TryParse(subscriptionId, out var planId))
                {
                    TempData["Error"] = "Invalid subscription plan.";
                    return RedirectToAction("Homepage", "Home");
                }
                var subscription = await _subscriptionPlanService.GetAsync(s => s.PlanId == planId);


                if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                {
                    TempData["Error"] = "An error occur during payment process, please try again";
                    return RedirectToAction("Homepage", "Home");
                }
                var user = await _userService.GetAsync(u => u.Id.Equals(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));

                //if (orderType.Equals("purchaseSubscription") && subscriptionId != null && user.SubscriptionPlan.Name.Equals("FREE"))
                if (orderType?.Equals("purchaseSubscription") == true &&
                    subscriptionId != null &&
                    (user.SubscriptionPlan?.Name?.Equals("FREE", StringComparison.OrdinalIgnoreCase) ?? true))

                {
                    var paymentRecord = new PaymentRecord
                    {
                        Amount = amount,
                        Currency = currency ?? "VND",
                        CreatedAt = DateTime.UtcNow,
                        Description = description,
                        Status = "Completed",
                        PaymentMethod = "VnPay",
                        UserId = user.Id,
                        ProcessedAt = DateTime.UtcNow,
                    };
                    await _paymentRecordService.AddAsync(paymentRecord);

                    var premium = await _subscriptionPlanService.GetAsync(s => s.Name.Equals("PREMIUM"));
                    user.SubscriptionStartDate = DateTime.UtcNow;
                    user.SubscriptionPlanId = premium.PlanId;
                    user.SubscriptionEndDate = (user.SubscriptionEndDate ?? DateTime.UtcNow).AddMonths(int.Parse(durationMonths ?? "0"));
                    await _userService.UpdateAsync(user);
                }
                else if (orderType.Equals("extendSubscription") && subscriptionId != null)
                {
                    var paymentRecord = new PaymentRecord
                    {
                        Amount = amount,
                        Currency = currency ?? "VND",
                        CreatedAt = DateTime.UtcNow,
                        Description = description,
                        Status = "Completed",
                        PaymentMethod = "VnPay",
                        UserId = user.Id,
                        ProcessedAt = DateTime.UtcNow,
                    };
                    await _paymentRecordService.AddAsync(paymentRecord);

                    user.SubscriptionEndDate = user.SubscriptionStartDate.Value.AddMonths(int.Parse(durationMonths ?? string.Empty));

                    await _userService.UpdateAsync(user);
                }

                else if (orderType.Equals("upgradeSubscription") && subscriptionId != null)
                {
                    //TODO: idk
                }

                // Clean up session
                HttpContext.Session.Remove($"OrderType_{txnRef}");
                HttpContext.Session.Remove($"SubscriptionId_{txnRef}");
                HttpContext.Session.Remove($"Currency_{txnRef}");
                HttpContext.Session.Remove($"DurationMonths_{txnRef}");

                TempData["Success"] = "Payment successful!";
                return RedirectToAction("Homepage", "Home");
            }
            else
            {
                TempData["Error"] = $"Payment failed, please try again";
                return RedirectToAction("Homepage", "Home");
            }
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProcessExtends(int PlanId, int DurationMonths, decimal Price, string OptionType, string currency = "USD", string ThirdParty = "VnPay")
        {

            var plan = await _subscriptionPlanService.GetAsync(p => p.PlanId == PlanId);
            if (plan == null)
            {
                TempData["Error"] = "An error while validating payment";
                return RedirectToAction("Homepage", "Home");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                TempData["Error"] = "An error while validating payment";
                return RedirectToAction("Homepage", "Home");
            }

            var user = await _userService.GetAsync(u => u.Id.Equals(userId));

            if (currency != null && currency.Equals("USD"))
            {
                Price *= 26000;
            }

            //switch (OptionType)
            //{
            //    case "purchase":
            //        {
            //            user.SubscriptionStartDate = DateTime.Now;
            //            user.SubscriptionEndDate = user.SubscriptionStartDate.Value.AddMonths(DurationMonths);
            //            break;
            //        }
            //    case "extension":
            //        {

            //            break;
            //        }

            //    case "upgrade":
            //        {
            //            break;
            //        }
            //}

            if (ThirdParty.Equals("VnPay"))
            {
                return RedirectToAction("CreatePaymentUrlVnpay", "Payment", new
                {
                    Amount = Price,
                    Name = User.FindFirst("FullName")?.Value,
                    OrderType = OptionType,
                    Currency = currency,
                    SubscriptionId = plan.PlanId,
                    OrderDescription = $"Payment subscription: {plan.DisplayName} via {ThirdParty} at LexiConnect",
                    DurationMonths,
                });
            }
            TempData["Error"] = "An error while validating payment";
            return RedirectToAction("Homepage", "Home");
        }
    }
}
