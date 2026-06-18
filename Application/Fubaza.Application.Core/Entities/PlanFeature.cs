namespace Fubaza.Application.Core.Entities
{
    public class PlanFeature
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? FeatureName { get; set; }  
        public Guid SubscriptionPlanId { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }
    }
}
