namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentRefGenerator
{
    string GeneratePaymentRef(bool isTopup = true);
    string GenerateRandomString(string prefix = "", int length = 12, bool onlyNumbers = false, bool onlyLetters = false);
}
