using SNEL.WalletPlatform.Domain.Payments;


namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public class SavePaymentInitializeResponseInputModel
{
    public string PaymentTransactionReference { get; set; }
    public bool IsSuccessfull { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public string ResponseBody { get; set; }
}
