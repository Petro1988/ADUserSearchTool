using Microsoft.Win32;

namespace ADUserSearchTool.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? GetExcelSaveFilePath(string defaultFileName)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Excel Export speichern",
                Filter = "Excel Datei (*.xlsx)|*.xlsx",
                FileName = defaultFileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
