namespace SubscriptionBilling.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Trial,
    PendingPayment,
    Active,
    PastDue,
    Cancelled,
    Expired
}
