namespace SNEL.WalletPlatform.Application.Common.Models.Input;

public class BaseQueryInputModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }

    public string? SortBy { get; set; }
    public bool IsAscending { get; set; } = true;
}
