namespace SubscriptionBilling.Domain;

public class Subscription
{
    public Guid Id;
    public Guid PlanId;
    public SubscriptionStatus Status { get; private set; }
    public DateTime TrialEndsAt { get; private set; }

    private Subscription(Guid planId, SubscriptionStatus status, DateTime trialEndsAt)
    {
        Id = Guid.NewGuid();
        PlanId = planId;
        Status = status;
        TrialEndsAt = trialEndsAt;
    }

    public static Subscription StartTrial(Guid planId, int trialDays, DateTime now)
    {
        return new Subscription(planId, SubscriptionStatus.Trial, now.AddDays(trialDays));
    }
}
