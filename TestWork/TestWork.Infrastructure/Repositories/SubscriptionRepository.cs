using Microsoft.EntityFrameworkCore;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Core.Models;
using TestWork.Infrastructure.Data;

namespace TestWork.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly TestWorkContext _context;

    public SubscriptionRepository(TestWorkContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string apartmentUrl, string email)
    {
        return await _context.Subscriptions.AnyAsync(subscription =>
            subscription.ApartmentUrl == apartmentUrl && subscription.Email == email);
    }

    public async Task AddAsync(SubscriptionModel subscription)
    {
        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SubscriptionModel>> GetAllAsync()
    {
        return await _context.Subscriptions
            .AsNoTracking()
            .OrderBy(subscription => subscription.Id)
            .ToListAsync();
    }

    public async Task<List<int>> GetIdsAsync()
    {
        return await _context.Subscriptions
            .AsNoTracking()
            .Select(subscription => subscription.Id)
            .ToListAsync();
    }

    public async Task<SubscriptionModel?> GetByIdAsync(int id)
    {
        return await _context.Subscriptions.FirstOrDefaultAsync(subscription => subscription.Id == id);
    }

    public async Task UpdateAsync(SubscriptionModel subscription)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }
}
