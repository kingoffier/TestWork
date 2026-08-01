namespace TestWork.Core.Models;

public class PriceCheckResultModel
{
    public SubscriptionModel Subscription { get; set; } = new();
    public bool PriceChanged { get; set; }
    public bool NotificationSent { get; set; }
}
