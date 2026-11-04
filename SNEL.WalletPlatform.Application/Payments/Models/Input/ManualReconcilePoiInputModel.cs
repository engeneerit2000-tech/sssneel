using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed record ManualReconcilePoiInputModel(
    [Required] Guid PaymentId,
    [Required] string ExternalRef);
