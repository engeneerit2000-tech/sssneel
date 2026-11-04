using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(InitializeClientSideFromWebNextActionModel), "InitializeClientSideFromWebNextActionModel")]
[JsonDerivedType(typeof(OtpConfirmationNextActionModel), "OtpConfirmationNextActionModel")]
[JsonDerivedType(typeof(PromptConfirmationNextActionModel), "PromptConfirmationNextActionModel")]
[JsonDerivedType(typeof(NoActionNextActionModel), "NoActionNextActionModel")]
[JsonDerivedType(typeof(ExtraInfoNextActionModel), "ExtraInfoNextActionModel")]
public interface IPaymentNextActionData
{
}
