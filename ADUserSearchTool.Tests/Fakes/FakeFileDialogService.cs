using ADUserSearchTool.Services;

namespace ADUserSearchTool.Tests.Fakes
{
    public class FakeFileDialogService : IFileDialogService
    {
        public string? FilePathToReturn { get; set; }

        public string? LastDefaultFileName { get; private set; }

        public string? GetExcelSaveFilePath(string defaultFileName)
        {
            LastDefaultFileName = defaultFileName;
            return FilePathToReturn;
        }
    }
}