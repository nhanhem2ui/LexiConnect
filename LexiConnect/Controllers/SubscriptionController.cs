using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace LexiConnect.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly IGenericRepository<SubscriptionPlan> _subscriptionPlanRepository;
        public SubscriptionController(IGenericRepository<SubscriptionPlan> subscriptionPlanRepository)
        {
            _subscriptionPlanRepository = subscriptionPlanRepository;
        }
        [HttpGet]
        public IActionResult Pricing()
        {
            var plans = _subscriptionPlanRepository.GetAllQueryable();
            var planFeatures = new List<PlanFeature>();

            foreach (var plan in plans)
            {
                var features = new List<string>();

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
                            ButtonClass = "btn-launch",
                            ButtonText = "Free Plan",
                            IsFree = true,
                            Features = features.AsQueryable(),
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
                            ButtonClass = "btn-advance",
                            ButtonText = "Premium Plan",
                            IsFree = false,
                            Features = features.AsQueryable(),
                        });
                        break;
                }
            }

            var model = new PricingViewModel()
            {
                Plans = planFeatures.AsQueryable(),
            };
            return View(model);
        }
    }
}
