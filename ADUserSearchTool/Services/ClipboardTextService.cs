using System.Windows;

namespace ADUserSearchTool.Services
{
    public class ClipboardTextService : IClipboardTextService
    {
        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}