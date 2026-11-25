namespace SubscriptionBilling.Domain.Subscriptions;

public class InvalidSubscriptionTransitionException(SubscriptionStatus currentStatus, string action) :
  Exception($"Cannot {action} when subscription is {currentStatus}.")
{
    public SubscriptionStatus CurrentStatus { get; } = currentStatus;
    public string Aciton { get; } = action;
}
