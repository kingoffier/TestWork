using System.ComponentModel.DataAnnotations;

namespace TestWork.API.Contracts.Subscription;

public record CreateRequest(
    [Required, Url] string ApartmentUrl,
    [Required, EmailAddress] string Email);
