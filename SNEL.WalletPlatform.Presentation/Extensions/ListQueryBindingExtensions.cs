using Microsoft.AspNetCore.Http;
using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Presentation.Extensions;

internal static class ListQueryBindingExtensions
{
    public static void ApplyListQueryFromQueryString(this BaseQueryInputModel model, IQueryCollection query)
    {
        if (query.TryGetValue("pageNumber", out var pageNumberValue)
            && int.TryParse(pageNumberValue.ToString(), out var pageNumber))
        {
            model.PageNumber = pageNumber;
        }

        if (query.TryGetValue("pageSize", out var pageSizeValue)
            && int.TryParse(pageSizeValue.ToString(), out var pageSize))
        {
            model.PageSize = pageSize;
        }

        if (query.TryGetValue("search", out var searchValue))
        {
            model.Search = searchValue.ToString();
        }

        if (query.TryGetValue("sortBy", out var sortByValue))
        {
            model.SortBy = sortByValue.ToString();
        }

        if (query.TryGetValue("isAscending", out var isAscendingValue)
            && bool.TryParse(isAscendingValue.ToString(), out var isAscending))
        {
            model.IsAscending = isAscending;
        }
        else if (query.TryGetValue("isDescending", out var isDescendingValue)
                 && bool.TryParse(isDescendingValue.ToString(), out var isDescending))
        {
            model.IsAscending = !isDescending;
        }
    }
}
