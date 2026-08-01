using TestWork.Core.Models;

namespace TestWork.Core.Interfaces.Subscription;

public interface ISubscriptionService
{
    Task<SubscriptionModel> CreateAsync(string apartmentUrl, string email);
    Task<List<SubscriptionModel>> GetAllAsync();
    Task CheckAllAsync();
    Task<PriceCheckResultModel?> UpdatePriceAsync(int id, decimal price);
    Task<PriceCheckResultModel?> CheckPriceAsync(int id);
}
