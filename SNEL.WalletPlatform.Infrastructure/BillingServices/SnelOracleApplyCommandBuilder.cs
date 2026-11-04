using System.Data;
using System.Text;
using Microsoft.Extensions.Localization;
using Oracle.ManagedDataAccess.Client;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices;

public sealed class SnelOracleApplyCommandBuilder(
    ISnelPaymentTypeResolver paymentTypeResolver,
    IStringLocalizer<SharedResource> localizer) : ISnelOracleApplyCommandBuilder
{
    public Result<IDbCommand> Build(
        IDbConnection connection,
        IReadOnlyList<PaymentOrderItemEntity> eligibleItems,
        decimal journalNumber,
        decimal startItemNumber,
        string posCode,
        decimal posId,
        string paymentMode)
    {
        if (eligibleItems.Count == 0)
        {
            return Result<IDbCommand>.Failure(
                localizer[LocalizationKeys.BillingApplyNoEligibleItems, Guid.Empty],
                AppHttpStatusCodes.BadRequest);
        }

        var command = (OracleCommand)connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.BindByName = true;

        var now = DateTime.UtcNow;
        var sql = new StringBuilder();
        sql.AppendLine("INSERT ALL");

        for (var i = 0; i < eligibleItems.Count; i++)
        {
            var item = eligibleItems[i];
            var currency = item.Currency!.Abbreviation;
            sql.AppendLine($"""
                INTO gco.GCO_REGLEMENTS
                (
                    IDE_REGLEMENT,
                    DAT_REGLEMENT,
                    DAT_VALEUR,
                    AMT_MONTANT_PAYE,
                    AMT_MT_PAYE_FC,
                    NUM_JOURNAL,
                    NUM_ITEM,
                    ENCO_IDE_ENCOURS,
                    PPRS_COD_PPR,
                    PPRS_IDE_PPR,
                    MODR_COD_MODE_REGL,
                    TYPR_TYP_REGLEMENT,
                    DEVI_COD_DEVISE
                )
                VALUES
                (
                    :reglementId{i},
                    :datReglement{i},
                    :datValeur{i},
                    :amountUsd{i},
                    :amountFc{i},
                    :journalNumber{i},
                    :itemNumber{i},
                    :originInvoiceId{i},
                    :pprCode{i},
                    :pprId{i},
                    :paymentMode{i},
                    :paymentType{i},
                    :currencyCode{i}
                )
                """);
            command.Parameters.Add(new OracleParameter($"reglementId{i}", OracleDbType.Decimal, 0m, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"datReglement{i}", OracleDbType.Date, now, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"datValeur{i}", OracleDbType.Date, now, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"amountUsd{i}", OracleDbType.Decimal, ResolveUsdAmount(item), ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"amountFc{i}", OracleDbType.Decimal, ResolveFcAmount(item), ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"diffChange{i}", OracleDbType.Decimal, 0m, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"journalNumber{i}", OracleDbType.Decimal, journalNumber, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"itemNumber{i}", OracleDbType.Decimal, startItemNumber + i, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"originInvoiceId{i}", OracleDbType.Decimal, item.Invoice?.OriginInvoiceId, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"pprCode{i}", OracleDbType.Varchar2, posCode, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"pprId{i}", OracleDbType.Decimal, posId, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"paymentMode{i}", OracleDbType.Varchar2, paymentMode, ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"paymentType{i}", OracleDbType.Varchar2, paymentTypeResolver.Resolve(item), ParameterDirection.Input));
            command.Parameters.Add(new OracleParameter($"currencyCode{i}", OracleDbType.Varchar2, currency, ParameterDirection.Input));
        }

        sql.Append("SELECT 1 FROM DUAL");
        command.CommandText = sql.ToString();
        return Result<IDbCommand>.Success(command);
    }

    private static decimal ResolveUsdAmount(PaymentOrderItemEntity item)
    {
        return item.Currency?.Abbreviation?.Equals("USD", StringComparison.OrdinalIgnoreCase) == true
            ? item.Amount
            : 0m;
    }

    private static decimal ResolveFcAmount(PaymentOrderItemEntity item)
    {
        return item.Currency?.Abbreviation?.Equals("USD", StringComparison.OrdinalIgnoreCase) == true
            ? 0m
            : item.Amount;
    }
}
