namespace SubscriptionBilling.Domain;

public enum SubscriptionStatus
{
    Trial,
    PendingPayment,
    Active,
    PastDue,
    Cancelled,
    Expired
}
