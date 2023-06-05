namespace Stayin.Core;

public class PaymentDetails
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required double Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}