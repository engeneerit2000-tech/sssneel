using System.Data;
using System.Globalization;
using System.Text.Json;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.Invoicing;

/// <summary>
/// Reads Oracle pull-invoices query row into OracleInvoiceRow; converts to Application OracleInvoiceRowModel for use with InvoiceMapper.
/// Invoice info is built from flat query columns and stored as JSON in staging.
/// </summary>
public static class OracleInvoiceMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
    private static Dictionary<string, int> ColumnIndexDictionary = new Dictionary<string, int>();

    /// <summary>
    /// Read current row from reader into OracleInvoiceRow. Column names are case-insensitive (Oracle returns uppercase).
    /// Builds InvoiceInfo and Payments JSON from flat columns (query no longer returns JSON).
    /// </summary>
    public static OracleInvoiceRowModel ReadRow(IDataRecord reader)
    {
        var invoiceId = GetLong(reader, "INVOICE_ID");
        var invoiceNum = GetString(reader, "INVOICE_NUM");
        var invoiceDate = GetDateTime(reader, "INVOICE_DATE");
        var invoiceUpdateDate = GetDateTime(reader, "INVOICE_UPDATE_DATE");
        var regionCode = GetString(reader, "REGION_CODE");
        var districtCode = GetString(reader, "DISTRICT_CODE");
        var customerId = GetDecimal(reader, "CUSTOMER_ID");
        var customerNum = GetString(reader, "CUSTOMER_NUM");
        var customerName = GetString(reader, "CUSTOMER_NAME");
        var customerTypeCode = GetString(reader, "CUSTOMER_TYPE_CODE");
        var customerTypeName = GetString(reader, "CUSTOMER_TYPE_NAME");
        var tariffType = GetString(reader, "TARIFF_TYPE");
        var billingPeriod = GetDecimal(reader, "BILLING_PERIOD");
        var consumption = GetDecimal(reader, "CONSUMPTION");
        var billedConsumption = GetDecimal(reader, "BILLED_CONSUMPTION");
        var invoiceCurrencyCode = GetString(reader, "INVOICE_CURRENCY_CODE");
        var amountUsd = GetDecimal(reader, "AMOUNT_USD");
        var amountFc = GetDecimal(reader, "AMOUNT_FC");
        var paidUsd = GetDecimal(reader, "PAID_USD");
        var paidFc = GetDecimal(reader, "PAID_FC");
        var paymentUpdateDate = GetDateTime(reader, "PAYMENT_UPDATE_DATE");
        var vat = GetDecimal(reader, "VAT");
        var provincialTax = GetDecimal(reader, "PROVINCIAL_TAX");
        var centerDesignation = GetString(reader, "CENTER_DESIGNATION");
        var pointOfSaleName = GetString(reader, "POINT_OF_SALE_NAME");
        var substationCode = GetString(reader, "SUBSTATION_CODE");
        var clientPropertyNumber = GetString(reader, "PROPERTY_NUMBER");
        var clientStreet = GetString(reader, "STREET");
        var clientCommune = GetString(reader, "COMMUNE");
        var meterNumber = GetDecimal(reader, "METER_NUMBER");
        var calculationCoefficient = GetDecimal(reader, "CALCULATION_COEFFICIENT");
        var previousBillingIndex = GetDecimal(reader, "PREVIOUS_BILLING_INDEX");
        var currentBillingIndex = GetDecimal(reader, "CURRENT_BILLING_INDEX");
        var discountCode = GetDecimal(reader, "DISCOUNT_CODE");
        var billingYear = GetString(reader, "PERIOD_YEAR");
        var billingMonth = GetString(reader, "PERIOD_MONTH");

        var taxAmount = (vat ?? 0) + (provincialTax ?? 0);
        var amountExcludingTaxUsd = amountUsd.HasValue ? amountUsd.Value - taxAmount : (decimal?)null;
        var amountExcludingTaxFc = amountFc.HasValue ? amountFc.Value - taxAmount : (decimal?)null;

        return new OracleInvoiceRowModel()
        {
            InvoiceId = invoiceId,
            InvoiceNum = invoiceNum,
            InvoiceDate = invoiceDate,
            InvoiceUpdateDate = invoiceUpdateDate,
            RegionCode = regionCode,
            DistrictCode = districtCode,
            CustomerId = customerId,
            CustomerNum = customerNum,
            CustomerName = customerName,
            CustomerTypeCode = customerTypeCode,
            CustomerTypeName = customerTypeName,
            TariffType = tariffType,
            BillingPeriod = billingPeriod,
            Consumption = consumption,
            BilledConsumption = billedConsumption,
            InvoiceCurrencyCode = invoiceCurrencyCode,
            AmountUsd = amountUsd,
            AmountFc = amountFc,
            PaidUsd = paidUsd,
            PaidFc = paidFc,
            PaymentUpdateDate = paymentUpdateDate,
            Vat = vat,
            ProvincialTax = provincialTax,
            CenterDesignation = centerDesignation,
            PointOfSaleName = pointOfSaleName,
            SubstationCode = substationCode,
            ClientPropertyNumber = clientPropertyNumber,
            ClientStreet = clientStreet,
            ClientCommune = clientCommune,
            MeterNumber = meterNumber,
            CalculationCoefficient = calculationCoefficient,
            PreviousBillingIndex = previousBillingIndex,
            CurrentBillingIndex = currentBillingIndex,
            DiscountCode = discountCode,
            AmountExcludingTaxUsd = amountExcludingTaxUsd,
            AmountExcludingTaxFc = amountExcludingTaxFc,
            SubtotalUsd = amountExcludingTaxUsd,
            SubtotalFc = amountExcludingTaxFc,
            BillingYear = string.IsNullOrWhiteSpace(billingYear)?null:int.Parse(billingYear),
            BillingMonth = string.IsNullOrWhiteSpace(billingMonth)?null:int.Parse(billingMonth),
        };
    }

  
    /// <summary>
    /// Convert InvoiceStagingEntity back to OracleInvoiceRow for the "insert new invoices" phase (read from staging in batches).
    /// </summary>
    public static OracleInvoiceRowModel FromStaging(InvoiceStagingEntity s)
    {
        return new OracleInvoiceRowModel
        {
            InvoiceId = s.OriginInvoiceId,
            InvoiceNum = s.InvoiceNum,
            InvoiceDate = s.InvoiceDate,
            InvoiceUpdateDate = s.InvoiceUpdateDate,
            RegionCode = s.RegionCode,
            DistrictCode = s.DistrictCode,
            CustomerId = s.CustomerId,
            CustomerNum = s.CustomerNum,
            CustomerName = s.CustomerName,
            CustomerTypeCode = s.CustomerTypeCode,
            CustomerTypeName = s.CustomerTypeName,
            TariffType = s.TariffType,
            BillingPeriod = s.BillingPeriod,
            Consumption = s.Consumption,
            BilledConsumption = s.BilledConsumption,
            InvoiceCurrencyCode = s.InvoiceCurrencyCode,
            AmountUsd = s.AmountUsd,
            AmountFc = s.AmountFc,
            PaidUsd = s.PaidUsd,
            PaidFc = s.PaidFc,
            PaymentUpdateDate = s.PaymentUpdateDate,
            Vat = s.Vat,
            ProvincialTax = s.ProvincialTax,
            CenterDesignation = s.CenterDesignation,
            Pa = s.Pa,
            PointOfSaleName = s.PointOfSaleName,
            SubstationCode = s.SubstationCode,
            ClientPropertyNumber = s.ClientPropertyNumber,
            ClientStreet = s.ClientStreet,
            ClientCommune = s.ClientCommune,
            MeterNumber = s.MeterNumber,
            CalculationCoefficient = s.CalculationCoefficient,
            PreviousBillingIndex = s.PreviousBillingIndex,
            CurrentBillingIndex = s.CurrentBillingIndex,
            DiscountCode = s.DiscountCode,
            AmountExcludingTaxUsd = s.AmountExcludingTaxUsd,
            AmountExcludingTaxFc = s.AmountExcludingTaxFc,
            SubtotalUsd = s.SubtotalUsd,
            SubtotalFc = s.SubtotalFc,
            BillingYear = s.BillingYear,
            BillingMonth = s.BillingMonth,
        };
    }

    private static string? GetString(IDataRecord reader, string columnName)
    {
        try
        {
            var ordinal = GetOrdinal(reader, columnName);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
                return null;
            return reader.GetString(ordinal);
        }
        catch(Exception ex)
        {
            var v = reader[columnName];
            return v?.ToString();
        }
    }

    private static decimal? GetDecimal(IDataRecord reader, string columnName)
    {
        try
        {
            var ordinal = GetOrdinal(reader, columnName);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
                return null;
            return reader.GetDecimal(ordinal);
        }
        catch(Exception ex)
        {
            var v = reader[columnName];
            if (v == null || v is DBNull)
                return null;
            if (v is decimal d) return d;
            if (v is double db) return (decimal)db;
            if (v is long l) return l;
            if (v is int i) return i;
            return decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }
    }

    private static int? GetInt(IDataRecord reader, string columnName)
    {
        var d = GetDecimal(reader, columnName);
        return d.HasValue ? decimal.ToInt32(d.Value) : null;
    }
    private static long? GetLong(IDataRecord reader, string columnName)
    {
        var d = GetDecimal(reader, columnName);
        return d.HasValue ? decimal.ToInt64(d.Value) : null;
    }
    

    private static DateTime? GetDateTime(IDataRecord reader, string columnName)
    {
        try
        {
            var ordinal = GetOrdinal(reader, columnName);
            if (ordinal < 0 || reader.IsDBNull(ordinal))
                return null;
            return reader.GetDateTime(ordinal);
        }
        catch(Exception ex)
        {
            var v = reader[columnName];
            if (v == null || v is DBNull)
                return null;
            if (v is DateTime dt) return dt;
            if (DateTime.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;
            return null;
        }
    }

    private static int GetOrdinal(IDataRecord reader, string columnName)
    {
        if (ColumnIndexDictionary.ContainsKey(columnName))
        {
            return ColumnIndexDictionary[columnName];
        }
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
            {
                ColumnIndexDictionary[columnName] = i;
                break;
            }
        }
        return ColumnIndexDictionary.GetValueOrDefault(columnName,-1);
    }
}
