namespace Fubaza.Application.Core.Entities
{
    public class SubscriptionPeriod
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? PeriodName { get; set; }
        public string? Description { get; set; }
        public ICollection<SubscriptionPlan> Plans { get; set; } = new List<SubscriptionPlan>();
    }
}
