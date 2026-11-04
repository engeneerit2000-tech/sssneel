using SNEL.WalletPlatform.Application.Payments.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SNEL.WalletPlatform.Application.Payments.Services;

public sealed class PaymentRefGenerator : IPaymentRefGenerator
{
    public string GeneratePaymentRef(bool isTopup)
    {
        string prefix = isTopup ? "TOPUP" : "PAYBill";

        return GenerateRandomString(prefix);
    }


    public string GenerateRandomString(
        string prefix="",
        int length = 12,
        bool onlyNumbers = false,
        bool onlyLetters = false)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");

        if (onlyNumbers && onlyLetters)
            throw new ArgumentException("onlyNumbers and onlyLetters cannot both be true.");

        const string numbers = "0123456789";
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        const string alphanumeric = letters + numbers;

        string chars = onlyNumbers
            ? numbers
            : onlyLetters
                ? letters
                : alphanumeric;

        var result = new StringBuilder(prefix.Length + length);
        result.Append(prefix);

        Span<byte> random = stackalloc byte[length];
        RandomNumberGenerator.Fill(random);

        for (int i = 0; i < length; i++)
        {
            int index = random[i] % chars.Length;
            result.Append(chars[index]);
        }

        return result.ToString();
    }
}