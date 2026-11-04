using SNEL.WalletPlatform.Application.Payments.Models.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal
{
    public record PaymentInitializationInternalModel(
    PaymentNextActionEnum PaymentNextAction,
    IPaymentNextActionData? Data);
}
