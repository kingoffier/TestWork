using System.Globalization;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Core.Models;
using TestWork.Infrastructure;

namespace TestWork.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendPriceChangedAsync(
        SubscriptionModel subscription,
        decimal oldPrice,
        decimal newPrice)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Тестовое уведомление для {Email}: цена {Url} изменилась с {OldPrice} на {NewPrice}",
                subscription.Email,
                subscription.ApartmentUrl,
                oldPrice,
                newPrice);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.From))
        {
            throw new InvalidOperationException("Для отправки писем нужно настроить Email:Host и Email:From.");
        }

        CultureInfo culture = CultureInfo.GetCultureInfo("ru-RU");
        using MailMessage message = new(_options.From, subscription.Email)
        {
            Subject = "Изменилась цена квартиры",
            Body = $"Цена квартиры {subscription.ApartmentName ?? subscription.ApartmentUrl} изменилась.\n\n" +
                   $"Старая цена: {oldPrice.ToString("N0", culture)} руб.\n" +
                   $"Новая цена: {newPrice.ToString("N0", culture)} руб.\n" +
                   $"Ссылка: {subscription.ApartmentUrl}"
        };

        using SmtpClient client = new(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        await client.SendMailAsync(message);
        return true;
    }
}
