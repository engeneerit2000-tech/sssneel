using System.Data;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Invoicing;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using SNEL.WalletPlatform.Infrastructure.Oracle;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Oracle implementation of <see cref="IInvoiceRetrieval"/> using the pull-invoices SQL (GCO_ENCOURS_FACTURES + payments).
/// </summary>
public sealed class PullInvoicesOracleQuery : IInvoiceRetrieval
{
    private const string PullInvoicesSql = """
                                           WITH invoices AS (
                                           SELECT
                                           	gef.IDE_ENCOURS invoice_id,
                                           	gef.NUM_FACTURE invoice_num,
                                           	gef.DAT_FACTURE invoice_date,
                                           	gef.DAT_UPD invoice_update_date,
                                           	gef.RACC_IDE_RACCORDEMENT customer_id,
                                           	gef.TCLI_TYP_CLIENT customer_type_code,
                                           	gef.TTAR_TYP_TARIFICATION tariff_type,
                                           	gef.PFAC_IDE_PER_FACT billing_period,
                                           	gef.QTY_CONSOMMATION_KWH consumption,
                                           	gef.QTY_CONS_FACTUREE billed_consumption,
                                           	gef.DEVI_COD_DEVISE invoice_currency_code,
                                           	gef.AMT_FACTURE amount_usd,
                                           	gef.AMT_FACT_FC amount_fc,
                                           	gef.AMT_ICA vat,
                                           	gef.AMT_TAXE_PROV provincial_tax,
                                           	gef.COMP_IDE_COMPTEUR meter_id,
                                           	greg.AMT_USD paid_usd,
                                           	greg.AMT_FC paid_fc,
                                           	greg.DAT_UPD payment_update_date
                                           FROM
                                           	gco.GCO_ENCOURS_FACTURES gef
                                           LEFT JOIN (
                                           	SELECT
                                           		x.ENCO_IDE_ENCOURS,
                                           		SUM(x.AMT_MONTANT_PAYE) AS AMT_USD,
                                           		SUM(x.AMT_MT_PAYE_FC) AS AMT_FC,
                                           		MAX(x.DAT_UPD) AS DAT_UPD
                                           	FROM
                                           		gco.GCO_REGLEMENTS x
                                           	GROUP BY
                                           		x.ENCO_IDE_ENCOURS
                                                                                               ) greg ON
                                           	greg.ENCO_IDE_ENCOURS = gef.IDE_ENCOURS
                                           WHERE
                                           	1 = 1
                                           	AND ((gef.DAT_UPD >= :START_TS
                                           		AND gef.DAT_UPD < :END_TS)
                                           	OR (greg.DAT_UPD >= :START_TS
                                           		AND greg.DAT_UPD < :END_TS))
                                           ORDER BY
                                           	gef.IDE_ENCOURS 
                                                                                      OFFSET (:page - 1) * :pageSize ROWS FETCH NEXT :pageSize ROWS ONLY)
                                           SELECT
                                           invs.*,
                                           	gr.REGI_COD_REGION region_code,
                                           	gr.DIRE_COD_DIRECTION district_code,
                                           	gr.NUM_RACCORDEMENT customer_num,
                                           	gr.NAM_RACCORDEMENT customer_name,
                                           	gtc.DES_CLIENT customer_type_name,
                                           	gtc_cent.DES_CENTRE center_designation,
                                           	gr.NUM_IMMEUBLE property_number,
                                           	grues.IDE_RUE street,
                                           	gcom.COD_COMMUNE commune,
                                           	gppa.DES_PPA point_of_sale_name,
                                           	gtc_racc.DES_CLIENT client_type_description,
                                           	gcab.NUM_CABINE substation_code,
                                           	COALESCE(gcompt.NUM_COMPTEUR, 0) meter_number,
                                           	COALESCE(gcompt.COEF_IDE_COEFF_MULTIPLI, 0) calculation_coefficient,
                                           	gtemp.COMP_QTY_INDEX_PREC previous_billing_index,
                                           	gtemp.COMP_QTY_INDEX_ACTU current_billing_index,
                                           	gtemp.RELI_QTY_CONS_CALCULEE calculated_kwh,
                                           	gtemp.QTY_RABAIS discount_code,
                                           	gtemp.RELI_QTY_CONS_FACTUREE billed_kwh,
                                           	gtt.TYP_TARIFICATION tariff_code,
                                           	gtemp.DAT_PERIODE_ANNEE period_year,
                                           	gtemp.DAT_PERIODE_MOIS period_month
                                           FROM
                                           	invoices invs
                                           JOIN gco.GCO_RACCORDEMENTS gr ON
                                           	gr.IDE_RACCORDEMENT = invs.customer_id
                                           JOIN gco.GCO_TYPE_CLIENTS gtc ON
                                           	gtc.TYP_CLIENT = invs.customer_type_code
                                           LEFT JOIN gco.GCO_TYPE_CLIENTS gtc_racc ON
                                           	gtc_racc.TYP_CLIENT = gr.TCLI_TYP_CLIENT
                                           LEFT JOIN gco.GCO_CENTRES gtc_cent ON
                                           	gtc_cent.COD_CENTRE = gr.CENT_COD_CENTRE
                                           LEFT JOIN gco.GCO_RUES grues ON
                                           	grues.IDE_RUE = gr.RUES_IDE_RUE
                                           LEFT JOIN gco.GCO_COMMUNES gcom ON
                                           	gcom.COD_COMMUNE = gr.COMM_COD_COMMUNE
                                           LEFT JOIN gco.GCO_PT_PERCEPTION_ASSIGNES gppa ON
                                           	gppa.COD_PPA = gr.PPAS_COD_PPA
                                           	AND gppa.CENT_COD_CENTRE = gr.CENT_COD_CENTRE
                                           LEFT JOIN gco.GCO_CABINES gcab ON
                                           	gcab.IDE_CABINE = gr.CABI_IDE_CABINE
                                           LEFT JOIN gco.GCO_COMPTEURS gcompt ON
                                           	gcompt.IDE_COMPTEUR = invs.meter_id
                                           LEFT JOIN gco.GCO_TMP_FACTURES gtemp ON
                                           	gtemp.NUM_FACTURE = SUBSTR(TRIM(invs.invoice_num), 4)
                                           LEFT JOIN gco.GCO_TYPE_TARIFICATIONS gtt ON
                                           	gtt.TYP_TARIFICATION = gtemp.TTAR_TYP_TARIF
                                           """;

    private readonly IOracleSnelConnectionFactory _oracleFactory;
    private readonly ILogger<PullInvoicesOracleQuery> _logger;
    private readonly SnelOracleLoggingOptions _loggingOptions;

    public PullInvoicesOracleQuery(IOracleSnelConnectionFactory oracleFactory,
        ILogger<PullInvoicesOracleQuery> logger,
        IOptions<SnelOracleLoggingOptions> loggingOptions)
    {
        _oracleFactory = oracleFactory;
        _logger = logger;
        _loggingOptions = loggingOptions.Value;
    }

    public async Task GetPageAsync(
        string areaCode,
        DateTime startTs,
        DateTime endTs,
        int pageIndex,
        int pageSize,
        Func<IReadOnlyList<OracleInvoiceRowModel>, CancellationToken, Task> onPageAsync,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var pageStopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=OpenOracleConnection started. AreaCode={AreaCode}, Page={Page}, PageSize={PageSize}.",
            areaCode, pageIndex, pageSize);
        using var connection = _oracleFactory.CreateConnection(areaCode);
        connection.Open();
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=OpenOracleConnection completed. AreaCode={AreaCode}, Page={Page}, ElapsedMs={ElapsedMs}.",
            areaCode, pageIndex, pageStopwatch.ElapsedMilliseconds);

        cancellationToken.ThrowIfCancellationRequested();

        using var cmd = (OracleCommand)connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = PullInvoicesSql;
        // Query can take several minutes on large ranges – allow up to 20 minutes.
        cmd.CommandTimeout = 1200;
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("START_TS", OracleDbType.TimeStamp, startTs, ParameterDirection.Input));
        cmd.Parameters.Add(new OracleParameter("END_TS", OracleDbType.TimeStamp, endTs, ParameterDirection.Input));
        cmd.Parameters.Add(new OracleParameter("page", OracleDbType.Int32, pageIndex, ParameterDirection.Input));
        cmd.Parameters.Add(new OracleParameter("pageSize", OracleDbType.Int32, pageSize,
            ParameterDirection.Input));

        SnelOracleCommandLogger.LogCommand(_logger, _loggingOptions.Enabled, "ExecuteReader", cmd);
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=ReadOraclePage started. AreaCode={AreaCode}, Page={Page}, StartTs={StartTs:O}, EndTs={EndTs:O}, PageSize={PageSize}.",
            areaCode, pageIndex, startTs, endTs, pageSize);

        var pageRows = new List<OracleInvoiceRowModel>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                pageRows.Add(OracleInvoiceMapper.ReadRow(reader));
                if (pageRows.Count % 10000 == 0)
                {
                    _logger.LogDebug(
                        "PullInvoicesOracleQuery: Stage=ReadOraclePage progress. AreaCode={AreaCode}, Page={Page}, RowsRead={RowsRead}, ElapsedMs={ElapsedMs}.",
                        areaCode, pageIndex, pageRows.Count, pageStopwatch.ElapsedMilliseconds);
                }
            }
        }

        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=ReadOraclePage completed. AreaCode={AreaCode}, Page={Page}, RowsCount={RowsCount}, StartTs={StartTs:O}, EndTs={EndTs:O}, ElapsedMs={ElapsedMs}.",
            areaCode, pageIndex, pageRows.Count, startTs, endTs, pageStopwatch.ElapsedMilliseconds);

        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=ProcessOraclePage started. AreaCode={AreaCode}, Page={Page}, RowsCount={RowsCount}.",
            areaCode, pageIndex, pageRows.Count);
        await onPageAsync(pageRows, cancellationToken);

        pageStopwatch.Stop();
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=ProcessOraclePage completed. AreaCode={AreaCode}, Page={Page}, RowsCount={RowsCount}, StartTs={StartTs:O}, EndTs={EndTs:O}, ElapsedMs={ElapsedMs}.",
            areaCode, pageIndex, pageRows.Count, startTs, endTs, pageStopwatch.ElapsedMilliseconds);
    }

    public async Task<long> GetAllAsync(
        string areaCode,
        DateTime startTs,
        DateTime endTs,
        int pageSize,
        string jobKey,
        string sourceSystem,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than zero.");
        }
        if (string.IsNullOrWhiteSpace(jobKey))
            throw new ArgumentException("jobKey must be non-empty.", nameof(jobKey));
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new ArgumentException("sourceSystem must be non-empty.", nameof(sourceSystem));

        var syncStopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=PullAllPages started. JobKey={JobKey}, SourceSystem={SourceSystem}, AreaCode={AreaCode}, StartTs={StartTs:O}, EndTs={EndTs:O}, PageSize={PageSize}.",
            jobKey, sourceSystem, areaCode, startTs, endTs, pageSize);

        var pageIndex = 1;
        long totalRows = 0L;
        while (true)
        {
            var fetchedCount = 0;
            await GetPageAsync(
                areaCode,
                startTs,
                endTs,
                pageIndex,
                pageSize,
                async (rows, ct) =>
                {
                    fetchedCount = rows.Count;
                    totalRows += fetchedCount;
                    if (fetchedCount == 0)
                    {
                        _logger.LogInformation(
                            "PullInvoicesOracleQuery: Stage=MapAndSavePrimaryStaging skipped because the Oracle page is empty. JobKey={JobKey}, SourceSystem={SourceSystem}, Page={Page}.",
                            jobKey, sourceSystem, pageIndex);
                        return;
                    }

                    var stagingStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation(
                        "PullInvoicesOracleQuery: Stage=MapOracleRows started. JobKey={JobKey}, SourceSystem={SourceSystem}, Page={Page}, RowsCount={RowsCount}.",
                        jobKey, sourceSystem, pageIndex, fetchedCount);
                    var stagingRows = rows.Select(x => InvoiceStagingEntity.FromOracleRow(
                            jobKey, sourceSystem,
                            x.InvoiceId, x.InvoiceNum, x.InvoiceDate, x.InvoiceUpdateDate,x.RegionCode, x.DistrictCode,
                            x.CustomerId, x.CustomerNum, x.CustomerName, x.CustomerTypeCode, x.CustomerTypeName,
                            x.TariffType, x.BillingPeriod, x.Consumption, x.BilledConsumption,
                            x.InvoiceCurrencyCode, x.AmountUsd, x.AmountFc, x.PaidUsd, x.PaidFc,
                            x.PaymentUpdateDate, x.Vat, x.ProvincialTax,
                            x.CenterDesignation, x.Pa, x.PointOfSaleName, x.SubstationCode,
                            x.ClientPropertyNumber, x.ClientStreet, x.ClientCommune,
                            x.MeterNumber, x.CalculationCoefficient, x.PreviousBillingIndex, x.CurrentBillingIndex,
                            x.DiscountCode, x.AmountExcludingTaxUsd, x.AmountExcludingTaxFc,
                            x.SubtotalUsd, x.SubtotalFc, x.BillingYear, x.BillingMonth))
                        .ToList();
                    _logger.LogInformation(
                        "PullInvoicesOracleQuery: Stage=MapOracleRows completed. JobKey={JobKey}, SourceSystem={SourceSystem}, Page={Page}, RowsCount={RowsCount}, ElapsedMs={ElapsedMs}.",
                        jobKey, sourceSystem, pageIndex, stagingRows.Count, stagingStopwatch.ElapsedMilliseconds);

                    _logger.LogInformation(
                        "PullInvoicesOracleQuery: Stage=SavePrimaryStaging started. JobKey={JobKey}, SourceSystem={SourceSystem}, Page={Page}, RowsCount={RowsCount}.",
                        jobKey, sourceSystem, pageIndex, stagingRows.Count);
                    dbContext.InvoiceStagings.AddRange(stagingRows);
                    await dbContext.SaveChangesAsync(ct);
                    dbContext.ChangeTracker.Clear();
                    stagingStopwatch.Stop();
                    _logger.LogInformation(
                        "PullInvoicesOracleQuery: Stage=SavePrimaryStaging completed. JobKey={JobKey}, SourceSystem={SourceSystem}, Page={Page}, RowsSaved={RowsSaved}, TotalRows={TotalRows}, Start={Start:O}, End={End:O}, ElapsedMs={ElapsedMs}.",
                        jobKey, sourceSystem, pageIndex, stagingRows.Count, totalRows, startTs, endTs,
                        stagingStopwatch.ElapsedMilliseconds);
                },
                cancellationToken);

            if (fetchedCount < pageSize)
            {
                _logger.LogInformation(
                    "PullInvoicesOracleQuery: Stage=Pagination completed. JobKey={JobKey}, SourceSystem={SourceSystem}, LastPage={LastPage}, LastPageRows={LastPageRows}, TotalRows={TotalRows}.",
                    jobKey, sourceSystem, pageIndex, fetchedCount, totalRows);
                break;
            }

            pageIndex++;
        }

        syncStopwatch.Stop();
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Stage=PullAllPages completed. JobKey={JobKey}, SourceSystem={SourceSystem}, PagesRead={PagesRead}, TotalRows={TotalRows}, ElapsedMs={ElapsedMs}.",
            jobKey, sourceSystem, pageIndex, totalRows, syncStopwatch.ElapsedMilliseconds);
        return totalRows;
    }
    
    public async Task<bool> TestConnectionAsync(
        string areaCode,
        CancellationToken cancellationToken = default)
    {
        using var connection = _oracleFactory.CreateConnection(areaCode);
        connection.Open();

        cancellationToken.ThrowIfCancellationRequested();

        using var cmd = (OracleCommand)connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "select * from GCO.GCO_DEVISES";
        cmd.CommandTimeout = 1200;
        
        _logger.LogInformation(
            "PullInvoicesOracleQuery: Testing Connection");
        var reader = cmd.ExecuteReader();
        try
        {
            if (reader.Read())
            {
                var obj = reader.GetValue(2);
                _logger.LogInformation(
                    "PullInvoicesOracleQuery: Testing Connection query result {obj}",obj);
                if (obj != null)
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(
                "PullInvoicesOracleQuery: Testing Connection Exception {msg}",e.Message);
        }
        return false;
    }
}
