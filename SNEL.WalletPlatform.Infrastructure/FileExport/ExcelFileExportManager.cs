using SNEL.WalletPlatform.Application.FileExport.Interfaces;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SNEL.WalletPlatform.Infrastructure.FileExport;

public class ExcelFileExportManager : IExcelFileExportManager
{
    public byte[] ExportToExcel(DataTable dataTable)
    {
        IWorkbook workbook = new XSSFWorkbook(); // Use HSSFWorkbook for .xls
        ISheet sheet = workbook.CreateSheet("Sheet1");

        // Create header row
        IRow headerRow = sheet.CreateRow(0);
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            headerRow.CreateCell(i).SetCellValue(dataTable.Columns[i].ColumnName);
        }

        // Fill data rows
        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            IRow row = sheet.CreateRow(i + 1);
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                row.CreateCell(j).SetCellValue(dataTable.Rows[i][j]?.ToString());
            }
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }
}
