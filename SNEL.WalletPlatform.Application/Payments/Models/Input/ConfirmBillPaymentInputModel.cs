using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed record ConfirmBillPaymentInputModel(
    [Required]Guid PaymentId);
