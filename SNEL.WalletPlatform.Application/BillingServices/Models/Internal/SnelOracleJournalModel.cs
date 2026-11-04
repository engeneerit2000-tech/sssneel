namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record SnelOracleJournalModel(
    decimal IdeCaisse,
    string PosCode,
    decimal NumJournal,
    DateTime DatJournal,
    DateTime DatOuverture,
    DateTime? DatCloture);
