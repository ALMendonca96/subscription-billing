namespace SubscriptionBilling.Domain.Subscriptions;

public class Subscription
{
    public Guid Id;
    public Guid PlanId;
    public SubscriptionStatus Status { get; private set; }
    public DateTime TrialEndsAt { get; private set; }
    public DateTime? CurrentPeriodEndsAt = null;

    private Subscription(Guid planId, SubscriptionStatus status, DateTime trialEndsAt)
    {
        Id = Guid.NewGuid();
        PlanId = planId;
        Status = status;
        TrialEndsAt = trialEndsAt;
    }

    public static Subscription StartTrial(Guid planId, int trialDays, DateTime now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trialDays);
        return new Subscription(planId, SubscriptionStatus.Trial, now.AddDays(trialDays));
    }

    public void RequestCheckout()
    {
        if (Status != SubscriptionStatus.Trial)
        {
            throw new InvalidSubscriptionTransitionException(Status, nameof(RequestCheckout));
        }

        Status = SubscriptionStatus.PendingPayment;
    }

    public void ConfirmPayment(DateTime periodEndsAt)
    {
        if (Status == SubscriptionStatus.Active && CurrentPeriodEndsAt == periodEndsAt) return;

        if (Status != SubscriptionStatus.PendingPayment)
        {
            throw new InvalidSubscriptionTransitionException(Status, nameof(RequestCheckout));
        }

        Status = SubscriptionStatus.Active;
        CurrentPeriodEndsAt = periodEndsAt;
    }
}
