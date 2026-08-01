using TestWork.Core.Models;

namespace TestWork.Core.Interfaces.Subscription;

public interface IEmailService
{
    Task<bool> SendPriceChangedAsync(
        SubscriptionModel subscription,
        decimal oldPrice,
        decimal newPrice);
}
