using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class SubscriptionExtensionViewModel
    {
        public string Title { get; set; } = "Extend Your Access";
        public string Subtitle { get; set; } = "Keep enjoying premium features";
        public string Description { get; set; } = "Continue your premium experience with flexible extension options.";
        public string SubDescription { get; set; } = "Choose to extend your current plan or upgrade to unlock even more features.";
        public string? MoneyBackGuarantee { get; set; } = "30-day money-back guarantee on all plans";
        public SubscriptionPlan CurrentPlan { get; set; } = new();
        public DateTime CurrentSubscriptionEndDate { get; set; }
        public int DaysRemaining => Math.Max(0, (CurrentSubscriptionEndDate - DateTime.UtcNow).Days);
        public List<string> CurrentPlanFeatures { get; set; } = new();
        public List<ExtensionOption> ExtensionOptions { get; set; } = new();
        public SubscriptionPlan? UpgradePlan { get; set; }
        public List<ExtensionOption> UpgradeOptions { get; set; } = new();
        public List<string> UpgradePlanFeatures { get; set; } = new();
        public bool IsCurrentlyFree { get; set; } = false;
        public bool HasUpgradeOption => UpgradePlan != null && UpgradeOptions.Count != 0;
    }

    public class ExtensionOption
    {
        public int PlanId { get; set; }
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int DiscountPercentage => OriginalPrice > 0 ? (int)Math.Round(((OriginalPrice - Price) / OriginalPrice) * 100) : 0;
        public bool IsRecommended { get; set; } = false;
        public DateTime NewEndDate { get; set; }

        // Calculated properties for display
        public string DisplayName => $"{DurationMonths} Month{(DurationMonths > 1 ? "s" : "")}";
        public bool HasDiscount => DiscountPercentage > 0;
    }
}