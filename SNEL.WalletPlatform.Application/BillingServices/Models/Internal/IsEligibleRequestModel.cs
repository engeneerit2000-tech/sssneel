using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal
{
    public sealed record IsEligibleRequestModel(
        string ServiceCode,
        string BillingRef,
        decimal Amount);
}
