using ADUserSearchTool.Models;
using System.Collections.Generic;

namespace ADUserSearchTool.Services
{
    public interface IExcelExportService
    {
        void ExportToExcel(string filePath, List<AdUserResult> users);
    }
}
