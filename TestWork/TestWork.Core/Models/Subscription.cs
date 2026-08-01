namespace TestWork.Core.Models;

public class SubscriptionModel
{
    public int Id { get; set; }
    public string ApartmentUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ApartmentName { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastCheckedAtUtc { get; set; }
}
