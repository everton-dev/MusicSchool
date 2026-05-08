using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Payments;

public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            return Result<Money>.Failure(new Error("Payment.AmountInvalid", "Payment amount must be greater than zero."));
        }

        if (decimal.Round(amount, 2) != amount)
        {
            return Result<Money>.Failure(new Error("Payment.AmountPrecisionInvalid", "Payment amount must not have more than two decimal places."));
        }

        var normalizedCurrency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency.Trim().ToUpperInvariant();
        if (normalizedCurrency != "EUR")
        {
            return Result<Money>.Failure(new Error("Payment.CurrencyUnsupported", "Only EUR payments are supported."));
        }

        return Result<Money>.Success(new Money(amount, normalizedCurrency));
    }
}
