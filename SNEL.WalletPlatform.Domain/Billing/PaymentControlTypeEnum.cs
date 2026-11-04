namespace SNEL.WalletPlatform.Domain.Billing
{
    public enum PaymentControlTypeEnum
    {
        AGGREGATE = 1, // Multiple payment controls per billing-service-id, billing-ref pair; amounts aggregated at apply
        UNIQUE = 2, // Single payment control per billing-service-id, billing-ref pair
    }
}