namespace SubscriptionBilling.Domain.Subscriptions;

public class Subscription
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime TrialEndsAt { get; private set; }
    public DateTime? CurrentPeriodEndsAt { get; private set; }
    public DateTime? GracePeriodEndsAt { get; private set; }

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
            throw new InvalidSubscriptionTransitionException(Status, nameof(ConfirmPayment));
        }

        Status = SubscriptionStatus.Active;
        CurrentPeriodEndsAt = periodEndsAt;
    }

    public void MarkOverdue(DateTime now, int graceDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(graceDays);

        if (Status == SubscriptionStatus.PastDue) return;

        if (Status != SubscriptionStatus.Active)
        {
            throw new InvalidSubscriptionTransitionException(Status, nameof(MarkOverdue));
        }

        Status = SubscriptionStatus.PastDue;
        GracePeriodEndsAt = now.AddDays(graceDays);
    }
}
