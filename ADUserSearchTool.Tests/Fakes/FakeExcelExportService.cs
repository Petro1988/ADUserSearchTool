using ADUserSearchTool.Models;
using ADUserSearchTool.Services;
using System.Collections.Generic;

namespace ADUserSearchTool.Tests.Fakes
{
    public class FakeExcelExportService : IExcelExportService
    {
        public bool WasCalled { get; private set; }

        public string? LastFilePath { get; private set; }

        public List<AdUserResult>? LastUsers { get; private set; }

        public void ExportToExcel(string filePath, List<AdUserResult> users)
        {
            WasCalled = true;
            LastFilePath = filePath;
            LastUsers = users;
        }
    }
}