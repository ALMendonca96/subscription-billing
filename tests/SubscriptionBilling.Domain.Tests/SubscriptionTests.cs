using SubscriptionBilling.Domain.Subscriptions;

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

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void StartTrial_with_invalid_days_throws(int trialDays)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Subscription.StartTrial(Guid.NewGuid(), trialDays, Now));
    }

    [Fact]
    public void RequestCheckout_from_Trial_moves_to_PendingPayment()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        subscription.RequestCheckout();

        Assert.Equal(SubscriptionStatus.PendingPayment, subscription.Status);
    }

    [Fact]
    public void RequestCheckout_when_already_PendingPayment_throws()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        subscription.RequestCheckout();

        var ex = Assert.Throws<InvalidSubscriptionTransitionException>(subscription.RequestCheckout);

        Assert.Equal(SubscriptionStatus.PendingPayment, ex.CurrentStatus);
        Assert.Equal("Cannot RequestCheckout when subscription is PendingPayment.", ex.Message);
    }

    [Fact]
    public void ConfirmPayment_from_PendingPayment_moves_to_Active()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        subscription.RequestCheckout();
        var periodEndsAt = Now.AddMonths(1);
        subscription.ConfirmPayment(periodEndsAt);

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(periodEndsAt, subscription.CurrentPeriodEndsAt);
    }

    [Fact]
    public void ConfirmPayment_twice_for_same_period_is_ignored()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        subscription.RequestCheckout();
        var periodEndsAt = Now.AddMonths(1);
        subscription.ConfirmPayment(periodEndsAt);
        subscription.ConfirmPayment(periodEndsAt);

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(periodEndsAt, subscription.CurrentPeriodEndsAt);
    }

    [Fact]
    public void ConfirmPayment_from_Trial_throws()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        var ex = Assert.Throws<InvalidSubscriptionTransitionException>(
            () => subscription.ConfirmPayment(Now.AddMonths(1)));

        Assert.Equal("Cannot ConfirmPayment when subscription is Trial.", ex.Message);
    }
}
