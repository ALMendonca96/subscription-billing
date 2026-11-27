namespace SubscriptionBilling.Domain.Subscriptions;

public enum CancellationReason
{
    TooExpensive = 0,
    MissingFeatures = 1,
    SwitchedToCompetitor = 2,
    NotUsing = 3,
    Other = 4
}
