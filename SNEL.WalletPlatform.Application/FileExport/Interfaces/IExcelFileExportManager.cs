using System.Data;

namespace SNEL.WalletPlatform.Application.FileExport.Interfaces;

public interface IExcelFileExportManager
{
    byte[] ExportToExcel(DataTable dataTable);
}
