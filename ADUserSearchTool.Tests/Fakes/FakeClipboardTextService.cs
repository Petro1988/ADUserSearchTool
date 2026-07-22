using ADUserSearchTool.Services;

namespace ADUserSearchTool.Tests.Fakes
{
    public class FakeClipboardTextService : IClipboardTextService
    {
        public string? LastText { get; private set; }

        public void SetText(string text)
        {
            LastText = text;
        }
    }
}