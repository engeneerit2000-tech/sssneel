using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

// Mutable runtime biller state lives here. SNEL currently uses a single POS for center 33
// until multi-center behavior is confirmed with stakeholders.
public sealed class BillerConfigEntity : TrackedEntity<long>
{
    private BillerConfigEntity()
    {
    }

    public string BillerCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string BillerName { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    public static BillerConfigEntity Create(
        string billerCode,
        string name,
        string billerName,
        string value)
    {
        return new BillerConfigEntity
        {
            BillerCode = billerCode.Trim(),
            Name = name.Trim(),
            BillerName = billerName.Trim(),
            Value = value.Trim()
        };
    }

    public void SetValue(string value) => Value = value.Trim();
}
