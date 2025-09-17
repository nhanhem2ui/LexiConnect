using BusinessObjects;
using System.Numerics;

namespace LexiConnect.Models.ViewModels
{
    public class PricingViewModel
    {
        public string Title { get; set; } = "All-In-One Price, Zero Hassle.";
        public string Subtitle { get; set; } = "Cancel Anytime. Let's Get Started!";
        public string Description { get; set; } = "Clear Pricing, No Strings Attached.";
        public string SubDescription { get; set; } = "Remember When SaaS Was This Simple? It's Time To Make It That Easy Again";
        public IQueryable<PlanFeature>? Plans { get; set; }
        public string MoneyBackGuarantee { get; set; } = "30-days money-back guarantee";
    }
    public class PlanFeature
    {
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public IQueryable<string>? Features { get; set; }
        public bool IsFree { get; set; }
        public string ButtonClass { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
    }
}
