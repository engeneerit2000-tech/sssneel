namespace SNEL.WalletPlatform.Application.Auth.Models.Options;

public class OtpConfigurationOptions
{
    public const string SectionName = "Otp";

    public int ExpireInMinutes { get; set; } 

    public int MaxAttempts { get; set; } 

    public int OtpLength { get; set; } 

    public required string DefaultOtp { get; set; }
}