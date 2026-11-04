using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed record ExecuteCustomActionsInputModel(
    string BillerCode, 
    string ServiceCode, 
    string BillingRef, 
    string ActionCode, 
    Dictionary<string, object> Parameters
);