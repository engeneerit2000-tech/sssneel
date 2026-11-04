namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public enum PaymentNextActionEnum
{
    None = 1,
    Confirm = 2,
    InitializeClientSideFromWeb = 3,
    ExtraInfoRequired = 4,
}
