using ADUserSearchTool.Services;

namespace ADUserSearchTool.Tests.Fakes
{
    public class FakeDialogService : IDialogService
    {
        public bool WasCalled { get; private set; }

        public string? LastTitle { get; private set; }

        public string? LastText { get; private set; }

        public void ShowGroupMemberships(string title, string text)
        {
            WasCalled = true;
            LastTitle = title;
            LastText = text;
        }
    }
}