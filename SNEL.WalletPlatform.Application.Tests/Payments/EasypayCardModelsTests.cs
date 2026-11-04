using System.Text.Json;
using SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;
using SNEL.WalletPlatform.Application.Webhooks.Models.Input;
using SNEL.WalletPlatform.Domain.Webhooks;

namespace SNEL.WalletPlatform.Application.Tests.Payments;

public sealed class EasypayCardModelsTests
{
    [Theory]
    [InlineData("USD")]
    [InlineData("CDF")]
    public void CardCollectionRequest_SerializesProviderFieldNames_AndOmitsNullOptionals(string currency)
    {
        var request = new EasypayCardCollectionRequestModel
        {
            ReferenceId = "INV-100246",
            Amount = 25m,
            Currency = currency,
            Description = "Invoice 100246"
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(request));
        var root = document.RootElement;

        Assert.Equal("INV-100246", root.GetProperty("referenceId").GetString());
        Assert.Equal(25m, root.GetProperty("amount").GetDecimal());
        Assert.Equal(currency, root.GetProperty("currency").GetString());
        Assert.Equal("Invoice 100246", root.GetProperty("description").GetString());
        Assert.False(root.TryGetProperty("email", out _));
        Assert.False(root.TryGetProperty("notify", out _));
        Assert.False(root.TryGetProperty("returnUrl", out _));
    }

    [Fact]
    public void CollectionResponse_DeserializesHostedPaymentLink()
    {
        const string json = """
            {
              "success": 1,
              "data": {
                "transactionId": "TX-4C91-4C93-1187",
                "referenceId": "INV-100246",
                "status": "Pending",
                "amount": 25,
                "currency": "USD",
                "provider": "Card",
                "paymentUrl": "https://www.easypay-gateway.com/payments/pay.php?ref=test",
                "expiresAt": "2026-11-02T10:15:00+00:00"
              }
            }
            """;

        var response = JsonSerializer.Deserialize<EasypayCollectionResponseModel>(json);

        Assert.NotNull(response);
        Assert.Equal(1, response.Success);
        Assert.Equal("TX-4C91-4C93-1187", response.Data?.TransactionId);
        Assert.Equal("Pending", response.Data?.Status);
        Assert.Equal("https://www.easypay-gateway.com/payments/pay.php?ref=test", response.Data?.PaymentUrl);
        Assert.Equal(new DateTimeOffset(2026, 11, 2, 10, 15, 0, TimeSpan.Zero), response.Data?.ExpiresAt);
    }

    [Theory]
    [InlineData("\"25.00\"")]
    [InlineData("25.00")]
    public void WebhookModel_AcceptsAmountAsStringOrNumber(string amountJson)
    {
        var json = $$"""
            {
              "transactionId": "TX-4C91-4C93-1187",
              "referenceId": "INV-100246",
              "status": "Success",
              "amount": {{amountJson}},
              "currency": "USD",
              "provider": "Card",
              "type": "Collections"
            }
            """;

        var input = JsonSerializer.Deserialize<EasypayWebhookEventInputModel>(json);

        Assert.NotNull(input);
        Assert.Equal(25m, input.Amount);
    }

    [Fact]
    public void WebhookEntity_PreservesCardEventDetails()
    {
        var webhookEvent = EasypayWebhookEventEntity.Create(
            "INV-100246",
            "Success",
            "TX-4C91-4C93-1187",
            25m,
            "USD",
            DateTimeOffset.Parse("2026-11-02T09:31:12Z"),
            "Card",
            string.Empty,
            "pi_3Q-test",
            "Card Dispute",
            "Dispute opened");

        Assert.Equal("Card Dispute", webhookEvent.Type);
        Assert.Equal("pi_3Q-test", webhookEvent.TelecomId);
        Assert.Equal("Dispute opened", webhookEvent.Reason);
        Assert.Null(webhookEvent.Phone);
    }
}
