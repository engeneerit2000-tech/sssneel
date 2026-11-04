using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using System.Runtime.CompilerServices;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentMethodEntity : TrackedEntity<long>
{
    //private PaymentMethodEntity()
    //{
    //    Code = string.Empty;
    //    Name = string.Empty;
    //    Config = null;
    //    PaymentInitMode = PaymentInitModeEnum.UNKNOWN;
    //    ConfirmationMode = ConfirmationModeEnum.UNKNOWN;
    //    CompletionMode = CompletionModeEnum.UNKNOWN;
    //    PaymentMethodPurpose = PaymentMethodPurposeEnum.UNKNOWN;
    //    ConfirmationTypeEnum = ConfirmationTypeEnum.NONE;
    //    PaymentMethodCurrencies = new List<PaymentMethodCurrencyEntity>();
    //    Payments = new List<PaymentEntity>();
    //}
    private PaymentMethodEntity() { }
    private PaymentMethodEntity(
        int paymentCategoryId,
        long paymentProviderId,
        string code,
        string name,
        PaymentInitModeEnum paymentInitMode,
        ConfirmationModeEnum confirmationMode,
        CompletionModeEnum completionMode,
        PaymentMethodPurposeEnum paymentMethodPurpose,
        bool active)
    {
        PaymentCategoryId = paymentCategoryId;
        PaymentProviderId = paymentProviderId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Active = active;
        Config = null;
        PaymentInitModeId = (int)paymentInitMode;
        ConfirmationModeId = (int)confirmationMode;
        CompletionModeId = (int)completionMode;
        PaymentMethodPurposeId = (int)paymentMethodPurpose;
        ConfirmationTypeId = (int)ConfirmationTypeEnum.NONE;
        PaymentMethodCurrencies = new List<PaymentMethodCurrencyEntity>();
        Payments = new List<PaymentEntity>();
    }

    public int PaymentCategoryId { get; private set; }
    public long PaymentProviderId { get; private set; }
    public bool Active { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public int PaymentInitModeId { get; private set; }
    public int ConfirmationModeId { get; private set; }
    public int CompletionModeId { get; private set; }
    public int PaymentMethodPurposeId { get; private set; }
    public int ConfirmationTypeId { get; private set; }
    public PaymentInitModeEntity? PaymentInitMode { get; private set; }
    public ConfirmationModeEntity? ConfirmationMode { get; private set; }
    public CompletionModeEntity? CompletionMode { get; private set; }
    public PaymentMethodPurposeEntity? PaymentMethodPurpose { get; private set; }
    public ConfirmationTypeEntity? ConfirmationType { get; private set; }
    public string? Config { get; private set; }

    public PaymentCategoryEntity? PaymentCategory { get; private set; }
    public PaymentProviderEntity? PaymentProvider { get; private set; }
    public ICollection<PaymentMethodCurrencyEntity> PaymentMethodCurrencies { get; private set; }
    public ICollection<PaymentEntity> Payments { get; private set; }

    public static PaymentMethodEntity Create(
        int paymentCategoryId,
        long paymentProviderId,
        string code,
        string name,
        PaymentMethodEnum paymentMethodCode,
        PaymentInitModeEnum paymentInitMode,
        ConfirmationModeEnum confirmationMode,
        CompletionModeEnum completionMode,
        PaymentMethodPurposeEnum paymentMethodPurpose,
        bool isActive = true)
    {
        if (paymentCategoryId == 0)
            throw new ArgumentException("PaymentCategoryId cannot be empty", nameof(paymentCategoryId));

        if (paymentProviderId == 0)
            throw new ArgumentException("PaymentProviderId cannot be empty", nameof(paymentProviderId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (paymentMethodPurpose == PaymentMethodPurposeEnum.UNKNOWN)
            throw new ArgumentException("Purpose cannot be UNKNOWN", nameof(paymentMethodPurpose));

        var paymentMethod= new PaymentMethodEntity(
            paymentCategoryId,
            paymentProviderId,
            code,
            name,
            paymentInitMode,
            confirmationMode,
            completionMode,
            paymentMethodPurpose,
            isActive);
        return paymentMethod;
    }

    public void Update(
        int paymentCategoryId,
        long paymentProviderId,
        string code,
        string name,
        PaymentInitModeEnum paymentInitMode,
        ConfirmationModeEnum confirmationMode,
        CompletionModeEnum completionMode,
        PaymentMethodPurposeEnum paymentMethodPurpose,
        ConfirmationTypeEnum confirmationTypeEnum,
        bool active,
        string? config)
    {
        PaymentCategoryId = paymentCategoryId;
        PaymentProviderId = paymentProviderId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        PaymentInitModeId = (int)paymentInitMode;
        ConfirmationModeId = (int)confirmationMode;
        CompletionModeId = (int)completionMode;
        PaymentMethodPurposeId = (int)paymentMethodPurpose;
        ConfirmationTypeId = (int)confirmationTypeEnum;
        Active = active;
        SetConfig(config);

    }

    public void Activate() => Active = true;
    public void Deactivate() => Active = false;
    public void SetConfig(string? config) =>
        Config = string.IsNullOrWhiteSpace(config) ? null : config.Trim();
    public void SetInitMode(PaymentInitModeEnum paymentInitMode) => PaymentInitModeId = (int)paymentInitMode;
    public void SetConfirmMode(ConfirmationModeEnum confirmationMode) => ConfirmationModeId = (int)confirmationMode;
    public void SetCompletionMode(CompletionModeEnum completionMode) => CompletionModeId = (int)completionMode;
    public void SetPurpose(PaymentMethodPurposeEnum paymentMethodPurpose) => PaymentMethodPurposeId = (int)paymentMethodPurpose;
    public void SetConfirmationType(ConfirmationTypeEnum confirmationType) => ConfirmationTypeId = (int)confirmationType;

    public int ValidateCompatibility()
    {
        var result = (PaymentInitModeEnum)PaymentInitModeId switch
        {
            PaymentInitModeEnum.CLIENT_SIDE => (CompletionModeEnum)CompletionModeId switch
            {
                CompletionModeEnum.IMMEDIATE => -10,
                CompletionModeEnum.POLLING => 0,
                CompletionModeEnum.WEBHOOK => 0,
            },
            PaymentInitModeEnum.SERVER_SIDE => (ConfirmationModeEnum)ConfirmationModeId switch
            {
                ConfirmationModeEnum.EXTERNAL => (CompletionModeEnum)CompletionModeId switch
                {
                    CompletionModeEnum.IMMEDIATE => -20,
                    CompletionModeEnum.POLLING => 0,
                    CompletionModeEnum.WEBHOOK => 0,
                },
                ConfirmationModeEnum.PLATFORM => 0,
                ConfirmationModeEnum.NONE => 0,
            },
            _ => -30
        };
        return result;
    }
}
