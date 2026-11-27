using SubscriptionBilling.Domain.Subscriptions;

namespace SubscriptionBilling.Domain.Tests;

public class SubscriptionTests
{
    private static readonly DateTime Now = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

    private static Subscription CreateActive()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        subscription.RequestCheckout();
        subscription.ConfirmPayment(Now.AddMonths(1));
        return subscription;
    }

    private static Subscription CreatePendingPayment()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        subscription.RequestCheckout();
        return subscription;
    }

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
        var subscription = CreatePendingPayment();

        Assert.Equal(SubscriptionStatus.PendingPayment, subscription.Status);
    }

    [Fact]
    public void RequestCheckout_when_already_PendingPayment_throws()
    {
        var subscription = CreatePendingPayment();
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

        Assert.Equal(SubscriptionStatus.Trial, ex.CurrentStatus);
        Assert.Equal("Cannot ConfirmPayment when subscription is Trial.", ex.Message);
    }

    [Fact]
    public void MarkOverdue_from_Active_moves_to_PastDue_and_sets_grace_end()
    {
        var subscription = CreateActive();
        subscription.MarkOverdue(Now, graceDays: 3);

        Assert.Equal(SubscriptionStatus.PastDue, subscription.Status);
        Assert.Equal(Now.AddDays(3), subscription.GracePeriodEndsAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MarkOverdue_with_invalid_grace_days_throws(int graceDays)
    {
        var subscription = CreateActive();
        Assert.Throws<ArgumentOutOfRangeException>(() => subscription.MarkOverdue(Now, graceDays));
    }

    [Fact]
    public void MarkOverdue_when_already_PastDue_does_not_extend_grace()
    {
        var subscription = CreateActive();
        subscription.MarkOverdue(Now, 3);
        subscription.MarkOverdue(Now.AddDays(1), 3);

        Assert.Equal(SubscriptionStatus.PastDue, subscription.Status);
        Assert.Equal(Now.AddDays(3), subscription.GracePeriodEndsAt);
    }

    [Fact]
    public void MarkOverdue_from_Trial_throws()
    {
        var subscription = Subscription.StartTrial(Guid.NewGuid(), 14, Now);
        var ex = Assert.Throws<InvalidSubscriptionTransitionException>(
            () => subscription.MarkOverdue(Now, 3));

        Assert.Equal(SubscriptionStatus.Trial, ex.CurrentStatus);
        Assert.Equal("Cannot MarkOverdue when subscription is Trial.", ex.Message);
    }

    [Fact]
    public void MarkOverdue_from_PendingPayment_throws()
    {
        var subscription = CreatePendingPayment();
        var ex = Assert.Throws<InvalidSubscriptionTransitionException>(
            () => subscription.MarkOverdue(Now, 3));

        Assert.Equal(SubscriptionStatus.PendingPayment, ex.CurrentStatus);
        Assert.Equal("Cannot MarkOverdue when subscription is PendingPayment.", ex.Message);
    }

    [Fact]
    public void Cancel_from_Active_moves_to_Cancelled_and_records_reason_and_date()
    {
        var subscription = CreateActive();
        subscription.Cancel(CancellationReason.MissingFeatures, Now);

        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Equal(Now, subscription.CancelledAt);
        Assert.Equal(CancellationReason.MissingFeatures, subscription.CancellationReason);
    }
}
