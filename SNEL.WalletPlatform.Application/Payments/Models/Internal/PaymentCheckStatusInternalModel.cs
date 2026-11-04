using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal
{
    public class PaymentCheckStatusInternalModel
    {
        public PaymentStatus Status { get; set; }
        public string PaymentExternalRef { get; set; } = default!;
        public string RequestBody { get; set; } = default!;
        public string ResponseBody { get; set; } = default!;
        public string ResponseCode { get; set; } = default!;
        public string ResponseMessage { get; set; } = default!;

        /// <summary>Optional localized message to return to the API caller. Must not contain PSP raw bodies.</summary>
        public string? UserMessage { get; set; }
    }
}
