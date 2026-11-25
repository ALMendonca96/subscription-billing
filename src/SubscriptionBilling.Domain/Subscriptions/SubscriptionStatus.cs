namespace SubscriptionBilling.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Trial = 0,
    PendingPayment = 1,
    Active = 2,
    PastDue = 3,
    Cancelled = 4,
    Expired = 5
}
