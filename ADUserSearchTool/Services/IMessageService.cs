namespace ADUserSearchTool.Services
{
    public interface IMessageService
    {
        void ShowInfo(string message, string title = "Information");
        void ShowWarning(string message, string title = "Warnung");
        void ShowError(string message, string title = "Fehler");
    }
}