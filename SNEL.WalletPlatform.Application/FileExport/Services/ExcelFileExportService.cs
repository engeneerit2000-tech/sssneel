using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.FileExport.Interfaces;
using SNEL.WalletPlatform.Application.Test;
using System.Data;

namespace SNEL.WalletPlatform.Application.FileExport.Services;

public class ExcelFileExportService (IExcelFileExportManager excelFileExportManager, IStringLocalizer<SharedResource> localizer)
{

    /// <summary>
    /// ReportName: will be the downloaded file prefix, <br />
    /// The model properties must be added in the resource files to get the transalted columns titles. <br />
    /// DataType is the type of the data object <br />
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="data"></param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public async Task<Result<FileContentResult>> ExportExcelFileAsync(string fileName, List<object> data, Type dataType)
    {
        // 1. Get Columns headers
        var columnsHeaders = dataType.GetProperties().Select(p => localizer.GetString(p.Name).ToString()).ToList();


        // 2. prepare input data
        var dataTable = new DataTable();
        foreach (var header in columnsHeaders)
        {
            dataTable.Columns.Add(header);
        }

        // Add data rows
        foreach (var item in data)
        {
            var row = dataTable.NewRow();
            var properties = dataType.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(item);
                row[i] = value?.ToString() ?? string.Empty;
            }

            dataTable.Rows.Add(row);
        }

        // 3. Get Excel arr
        var fileByteArr = excelFileExportManager.ExportToExcel(dataTable);

        // 4. Prepare return file
        var file = new FileContentResult(fileByteArr, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"{fileName}.xlsx"
        };

        return Result<FileContentResult>.Success(file);
    }
}
