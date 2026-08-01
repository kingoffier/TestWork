using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Infrastructure;

namespace TestWork.Application.Services;

public class PriceCheckerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PriceTrackingOptions _options;
    private readonly ILogger<PriceCheckerBackgroundService> _logger;

    public PriceCheckerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<PriceTrackingOptions> options,
        ILogger<PriceCheckerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int minutes = Math.Max(1, _options.CheckIntervalMinutes);
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(minutes));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    ISubscriptionService subscriptionService =
                        scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                    await subscriptionService.CheckAllAsync();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Ошибка фоновой проверки цен");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Фоновая проверка цен остановлена");
        }
    }
}
