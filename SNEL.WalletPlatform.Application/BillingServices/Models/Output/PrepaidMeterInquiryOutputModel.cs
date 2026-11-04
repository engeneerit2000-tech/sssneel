namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed record PrepaidMeterInquiryOutputModel(
    string MeterNumber,
    string CustomerName);
