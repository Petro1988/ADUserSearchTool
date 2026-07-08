namespace ADUserSearchTool.ViewModels
{
    public class GroupMembershipViewModel
    {
        public string Title { get; }
        public string Text { get; }

        public GroupMembershipViewModel(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}