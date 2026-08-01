using Microsoft.AspNetCore.Mvc;
using TestWork.API.Contracts.Subscription;
using TestWork.Application;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Core.Models;

namespace TestWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost("createSubscription")]
    [ProducesResponseType<SubscriptionModel>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionAsync(CreateRequest subscription)
    {
        try
        {
            SubscriptionModel result = await _subscriptionService.CreateAsync(
                subscription.ApartmentUrl,
                subscription.Email);

            return Created($"/api/subscription/{result.Id}", result);
        }
        catch (DuplicateSubscriptionException)
        {
            return Conflict(new { message = "Такая подписка уже существует." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (HttpRequestException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Не удалось загрузить страницу квартиры.",
                details = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return UnprocessableEntity(new { message = exception.Message });
        }
    }

    [HttpGet("getAllSubscriptions")]
    [ProducesResponseType<List<SubscriptionModel>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSubscriptionsAsync([FromQuery] bool refresh = false)
    {
        if (refresh)
        {
            await _subscriptionService.CheckAllAsync();
        }

        List<SubscriptionModel> subscriptions = await _subscriptionService.GetAllAsync();
        return Ok(subscriptions);
    }

    [HttpPatch("updatePrice/{id:int}")]
    [ProducesResponseType<PriceCheckResultModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePriceAsync(int id, UpdateRequest subscription)
    {
        if (subscription.Price <= 0)
        {
            ModelState.AddModelError(nameof(subscription.Price), "Цена должна быть больше нуля.");
            return ValidationProblem(ModelState);
        }

        try
        {
            PriceCheckResultModel? result = await _subscriptionService
                .UpdatePriceAsync(id, subscription.Price);

            return result is null ? NotFound() : Ok(result);
        }
        catch (HttpRequestException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Не удалось загрузить страницу квартиры.",
                details = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return UnprocessableEntity(new { message = exception.Message });
        }
    }
}
