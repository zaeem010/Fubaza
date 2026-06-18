namespace Fubaza.Application.Core.Entities
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? PlanName { get; set; }    
        public decimal MonthlyRate { get; set; } 
        public string? CurrencyCode { get; set; } 
        public bool HasFreeTrial { get; set; }
        public Guid SubscriptionPeriodId { get; set; }
        public SubscriptionPeriod?  SubscriptionPeriod { get; set; }
        public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
    }
}
