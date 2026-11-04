using SNEL.WalletPlatform.Application.Common.Models.Input;
using System.ComponentModel.DataAnnotations;
using ZXing.Client.Result;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

public sealed class JournalLinesStatementInputModel : BaseQueryInputModel
{    
    public long? PartnerId { get; set; }
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; set; }
    public int? AccountTypeId { get; set; }
    public int? JournalLineTypeId { get; set; }
    
    [Required]
    public DateTimeOffset FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
}

public sealed class JournalLinesStatementInternalModel : BaseQueryInputModel
{
    public long? PartnerId { get; set; }
    public int? CurrencyId { get; set; }
    public int? AccountTypeId { get; set; }
    public int? JournalLineTypeId { get; set; }
    public bool AddingFirstRecord { get; set; } = false;

    [Required]
    public DateTimeOffset FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
}