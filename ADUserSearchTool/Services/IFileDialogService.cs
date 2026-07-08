namespace ADUserSearchTool.Services
{
    public interface IFileDialogService
    {
        string? GetExcelSaveFilePath(string defaultFileName);
    }
}