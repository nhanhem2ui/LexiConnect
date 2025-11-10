using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Claims;

namespace LexiConnect.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly IGenericService<SubscriptionPlan> _subscriptionPlanService;
        private readonly IGenericService<Users> _userService;

        public SubscriptionController(IGenericService<SubscriptionPlan> subscriptionPlanService,
            IGenericService<Users> userService)
        {
            _subscriptionPlanService = subscriptionPlanService;
            _userService = userService;
        }

        [HttpGet]

        public async Task<IActionResult> Pricing()
        {
            var plans = _subscriptionPlanService.GetAllQueryable();
            var planFeatures = new List<PlanFeature>();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = new Users();
            bool hasActivePremium = false;
            DateTime? subscriptionEndDate = null;

            if (userId != null)
            {
                currentUser = await _userService.GetAsync(u => u.Id.Equals(userId));
                if (currentUser?.SubscriptionPlanId != null && currentUser.SubscriptionEndDate > DateTime.UtcNow)
                {
                    hasActivePremium = true;
                    subscriptionEndDate = currentUser.SubscriptionEndDate;
                }
            }

            foreach (var plan in plans)
            {
                var features = new List<string>();
                bool isCurrentPlan = currentUser?.SubscriptionPlanId == plan.PlanId;

                switch (plan.Name)
                {
                    case "FREE":
                        features.Add("Partial access of the library; previews or limited content.");
                        features.Add("Free users can upload documents and gain points when accepted.");
                        features.Add("Likely ads or prompts to upgrade.");
                        features.Add("Basic customer service support. Longer response times.");

                        planFeatures.Add(new PlanFeature
                        {
                            SubscriptionPlan = plan,
                            ButtonClass = isCurrentPlan ? "btn-current" : "btn-launch",
                            ButtonText = isCurrentPlan ? "Current Plan" : "Free Plan",
                            IsFree = true,
                            Currency = plan.Currency,
                            IsCurrentPlan = isCurrentPlan,
                            Features = features.AsQueryable(),
                            CardClass = isCurrentPlan ? "current-plan" : ""
                        });
                        break;

                    case "PREMIUM":
                        features.Add("Full access to the entire document library; unlock premium materials.");
                        features.Add("Premium ongoing access via subscription, plus upload rewards if desired.");
                        features.Add("Ad-free experience.");
                        features.Add("Priority support; faster help with issues.");

                        planFeatures.Add(new PlanFeature
                        {
                            SubscriptionPlan = plan,
                            ButtonClass = isCurrentPlan ? "btn-current" : "btn-advance",
                            ButtonText = isCurrentPlan ? "Current Plan" : "Upgrade to Premium",
                            IsFree = false,
                            Currency = plan.Currency,
                            IsCurrentPlan = isCurrentPlan,
                            Features = features.AsQueryable(),
                            CardClass = isCurrentPlan ? "current-plan" : ""
                        });
                        break;
                }
            }

            var model = new PricingViewModel()
            {
                Plans = planFeatures.AsQueryable(),
                HasActivePremium = hasActivePremium,
                SubscriptionEndDate = subscriptionEndDate,
                CurrentUser = currentUser
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExtendSubscription()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Signin", "Auth");
            }

            var currentUser = await _userService.GetAsync(u => u.Id.Equals(userId));

            SubscriptionPlan? currentPlan = null;
            List<ExtensionOption> extensionOptions = new();
            List<ExtensionOption> upgradeOptions = new();
            SubscriptionPlan? upgradePlan = null;
            SubscriptionPlan? premiumPlan = null;

            bool isCurrentlyFree = currentUser?.SubscriptionPlanId == null ||
                                   (currentUser.SubscriptionEndDate.HasValue && currentUser.SubscriptionEndDate.Value <= DateTime.UtcNow) ||
                                   currentUser?.SubscriptionEndDate == DateTime.MinValue ||
                                   currentUser?.SubscriptionEndDate == null;

            if (isCurrentlyFree)
            {
                // For free users, get the premium plan options
                premiumPlan = await _subscriptionPlanService.GetAsync(p => p.Name.Equals("PREMIUM"));
                if (premiumPlan != null)
                {
                    extensionOptions = GetPremiumPurchaseOptions(premiumPlan);
                }
            }
            else
            {
                // For premium users, show extension options
                if (currentUser?.SubscriptionPlanId != null &&
                    currentUser.SubscriptionEndDate.HasValue &&
                    currentUser.SubscriptionEndDate.Value > DateTime.UtcNow)
                {
                    currentPlan = await _subscriptionPlanService.GetAsync(p => p.PlanId == currentUser.SubscriptionPlanId);
                    if (currentPlan != null)
                    {
                        extensionOptions = GetExtensionOptions(currentPlan, currentUser.SubscriptionEndDate.Value);
                        upgradePlan = await GetUpgradePlanAsync(currentPlan);

                        if (upgradePlan != null)
                        {
                            upgradeOptions = GetUpgradeOptions(upgradePlan, currentUser.SubscriptionEndDate.Value);
                        }
                    }
                }
            }

            // Build ViewModel
            var viewModel = new SubscriptionExtensionViewModel
            {
                CurrentPlan = isCurrentlyFree ?
                    (await _subscriptionPlanService.GetAsync(s => s.Name.Equals("FREE")) ?? new()) :
                    (currentPlan ?? new()),
                CurrentSubscriptionEndDate = currentUser?.SubscriptionEndDate ?? DateTime.MinValue,
                CurrentPlanFeatures = isCurrentlyFree ? new List<string>
                {
                    "You are currently on the Free Plan.",
                    "Upgrade to access premium content and unlock all features."
                } : (currentPlan != null ? GetPlanFeatures(currentPlan) : new List<string>()),
                ExtensionOptions = extensionOptions,
                UpgradePlan = upgradePlan,
                UpgradeOptions = upgradeOptions,
                UpgradePlanFeatures = isCurrentlyFree ?
                    (premiumPlan != null ? GetPlanFeatures(premiumPlan) : new List<string>()) :
                    (upgradePlan != null ? GetPlanFeatures(upgradePlan) : new List<string>()),
                MoneyBackGuarantee = "30-day money-back guarantee on all subscriptions",
                IsCurrentlyFree = isCurrentlyFree
            };

            return View(viewModel);
        }

        private static List<ExtensionOption> GetPremiumPurchaseOptions(SubscriptionPlan premiumPlan)
        {
            var options = new List<ExtensionOption>();
            var durations = new[] { 1, 3, 6, 12 };

            for (int i = 0; i < durations.Length; i++)
            {
                var duration = durations[i];
                var basePrice = premiumPlan.Price * duration;
                var discountedPrice = ApplyBulkDiscount(basePrice, duration);

                options.Add(new ExtensionOption
                {
                    PlanId = premiumPlan.PlanId,
                    DurationMonths = duration,
                    Price = discountedPrice,
                    OriginalPrice = basePrice,
                    IsRecommended = duration == 3, // 3 months is recommended
                    NewEndDate = DateTime.UtcNow.AddMonths(duration)
                });
            }

            return options;
        }

        private static List<ExtensionOption> GetExtensionOptions(SubscriptionPlan currentPlan, DateTime currentEndDate)
        {
            var options = new List<ExtensionOption>();
            var durations = new[] { 1, 3, 6, 12 };

            for (int i = 0; i < durations.Length; i++)
            {
                var duration = durations[i];
                var basePrice = currentPlan.Price * duration;
                var discountedPrice = ApplyBulkDiscount(basePrice, duration);

                options.Add(new ExtensionOption
                {
                    PlanId = currentPlan.PlanId,
                    DurationMonths = duration,
                    Price = discountedPrice,
                    OriginalPrice = basePrice,
                    IsRecommended = duration == 3, // 3 months is recommended
                    NewEndDate = currentEndDate.AddMonths(duration)
                });
            }

            return options;
        }

        private async Task<SubscriptionPlan?> GetUpgradePlanAsync(SubscriptionPlan currentPlan)
        {
            // Get the next higher tier plan
            var upgradePlan = await _subscriptionPlanService
                .GetAllQueryable(p => p.IsActive && p.Price > currentPlan.Price && p.PlanId != currentPlan.PlanId, asNoTracking: true)
                .OrderBy(p => p.Price)
                .FirstOrDefaultAsync();

            return upgradePlan;
        }

        private List<ExtensionOption> GetUpgradeOptions(SubscriptionPlan upgradePlan, DateTime currentEndDate)
        {
            var options = new List<ExtensionOption>();
            var durations = new[] { 1, 3, 6, 12 };

            for (int i = 0; i < durations.Length; i++)
            {
                var duration = durations[i];
                var basePrice = upgradePlan.Price * duration;
                var discountedPrice = ApplyBulkDiscount(basePrice, duration);

                options.Add(new ExtensionOption
                {
                    PlanId = upgradePlan.PlanId,
                    DurationMonths = duration,
                    Price = discountedPrice,
                    OriginalPrice = basePrice,
                    IsRecommended = duration == 3, // 3 months is recommended
                    NewEndDate = currentEndDate.AddMonths(duration)
                });
            }

            return options;
        }

        private static decimal ApplyBulkDiscount(decimal basePrice, int durationMonths)
        {
            // Apply bulk discounts
            return durationMonths switch
            {
                1 => basePrice, // No discount for 1 month
                3 => basePrice * 0.89m, // 11% discount for 3 months
                6 => basePrice * 0.83m, // 17% discount for 6 months
                12 => basePrice * 0.78m, // 22% discount for 12 months
                _ => basePrice
            };
        }

        private static List<string> GetPlanFeatures(SubscriptionPlan plan)
        {
            var features = new List<string>();

            if (plan.MaxDownloadsPerMonth.HasValue)
            {
                features.Add($"{plan.MaxDownloadsPerMonth} downloads per month");
            }
            else
            {
                features.Add("Unlimited downloads per month");
            }

            if (plan.PremiumContentAccess)
            {
                features.Add("Premium content access");
                features.Add("Exclusive premium resources");
            }
            else
            {
                features.Add("Standard content access");
            }

            if (plan.PrioritySupport)
            {
                features.Add("Priority customer support");
                features.Add("24/7 dedicated support");
            }
            else
            {
                features.Add("Standard customer support");
            }
            return features;
        }
    }
}