using TestWork.Core.Models;

namespace TestWork.Core.Interfaces.Subscription;

public interface ISubscriptionRepository
{
    Task<bool> ExistsAsync(string apartmentUrl, string email);
    Task AddAsync(SubscriptionModel subscription);
    Task<List<SubscriptionModel>> GetAllAsync();
    Task<List<int>> GetIdsAsync();
    Task<SubscriptionModel?> GetByIdAsync(int id);
    Task UpdateAsync(SubscriptionModel subscription);
}
