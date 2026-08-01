using Microsoft.Extensions.Logging;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Core.Models;

namespace TestWork.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private static readonly SemaphoreSlim CheckLock = new(1, 1);

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IApartmentPriceParser _apartmentPriceParser;
    private readonly IEmailService _emailService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IApartmentPriceParser apartmentPriceParser,
        IEmailService emailService,
        ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _apartmentPriceParser = apartmentPriceParser;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SubscriptionModel> CreateAsync(string apartmentUrl, string email)
    {
        string normalizedUrl = ApartmentUrlHelper.NormalizeAndValidate(apartmentUrl);
        string normalizedEmail = email.Trim().ToLowerInvariant();
        ParsedApartmentModel apartment = await _apartmentPriceParser.GetApartmentAsync(normalizedUrl);

        if (await _subscriptionRepository.ExistsAsync(normalizedUrl, normalizedEmail))
        {
            throw new DuplicateSubscriptionException();
        }

        DateTime now = DateTime.UtcNow;
        SubscriptionModel subscription = new()
        {
            ApartmentUrl = normalizedUrl,
            Email = normalizedEmail,
            ApartmentName = apartment.Name,
            CurrentPrice = apartment.Price,
            CreatedAtUtc = now,
            LastCheckedAtUtc = now
        };

        await _subscriptionRepository.AddAsync(subscription);
        return subscription;
    }

    public async Task<List<SubscriptionModel>> GetAllAsync()
    {
        return await _subscriptionRepository.GetAllAsync();
    }

    public async Task CheckAllAsync()
    {
        List<int> subscriptionIds = await _subscriptionRepository.GetIdsAsync();

        foreach (int id in subscriptionIds)
        {
            try
            {
                await CheckPriceAsync(id);
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
            {
                _logger.LogError(exception, "Не удалось проверить подписку {SubscriptionId}", id);
            }
        }
    }

    public async Task<PriceCheckResultModel?> UpdatePriceAsync(int id, decimal price)
    {
        SubscriptionModel? subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription is null)
        {
            return null;
        }

        subscription.CurrentPrice = price;
        await _subscriptionRepository.UpdateAsync(subscription);
        return await CheckPriceAsync(id);
    }

    public async Task<PriceCheckResultModel?> CheckPriceAsync(int id)
    {
        await CheckLock.WaitAsync();

        try
        {
            SubscriptionModel? subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription is null)
            {
                return null;
            }

            decimal oldPrice = subscription.CurrentPrice;
            ParsedApartmentModel apartment = await _apartmentPriceParser.GetApartmentAsync(subscription.ApartmentUrl);
            bool priceChanged = oldPrice != apartment.Price;
            bool notificationSent = false;

            if (priceChanged)
            {
                notificationSent = await _emailService.SendPriceChangedAsync(
                    subscription,
                    oldPrice,
                    apartment.Price);
            }

            subscription.CurrentPrice = apartment.Price;
            subscription.ApartmentName = apartment.Name;
            subscription.LastCheckedAtUtc = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription);

            return new PriceCheckResultModel
            {
                Subscription = subscription,
                PriceChanged = priceChanged,
                NotificationSent = notificationSent
            };
        }
        finally
        {
            CheckLock.Release();
        }
    }
}
