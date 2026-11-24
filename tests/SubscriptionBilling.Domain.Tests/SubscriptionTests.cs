namespace SubscriptionBilling.Domain.Tests;

public class SubscriptionTests
{
    private static readonly DateTime Now = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void StartTrial_sets_status_to_Trial_and_trial_end_date()
    {
        var planId = Guid.NewGuid();
        var subscription = Subscription.StartTrial(planId, trialDays: 14, now: Now);

        Assert.Equal(SubscriptionStatus.Trial, subscription.Status);
        Assert.Equal(planId, subscription.PlanId);
        Assert.Equal(Now.AddDays(14), subscription.TrialEndsAt);
    }
}
