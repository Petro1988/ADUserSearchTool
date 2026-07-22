using ADUserSearchTool.Services;

namespace ADUserSearchTool.Tests.Fakes
{
    public class FakeMessageService : IMessageService
    {
        public string? LastMessage { get; private set; }

        public string? LastTitle { get; private set; }

        public void ShowInfo(string message, string title = "Information")
        {
            LastMessage = message;
            LastTitle = title;
        }

        public void ShowWarning(string message, string title = "Warnung")
        {
            LastMessage = message;
            LastTitle = title;
        }

        public void ShowError(string message, string title = "Fehler")
        {
            LastMessage = message;
            LastTitle = title;
        }
    }
}