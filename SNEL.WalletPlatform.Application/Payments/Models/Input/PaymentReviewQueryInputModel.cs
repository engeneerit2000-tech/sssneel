using SNEL.WalletPlatform.Application.Common.Models.Input;
using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class PaymentReviewQueryInputModel : BaseQueryInputModel
{
    public long? BillerId { get; set; } = null;
}