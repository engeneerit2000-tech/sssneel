namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record SnelOracleJournalAllocationModel(
    decimal IdeCaisse,
    string PosCode,
    decimal NumJournal,
    decimal NextItemNumber,
    DateTime DatJournal);
