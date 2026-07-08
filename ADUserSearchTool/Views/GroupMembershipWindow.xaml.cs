using System.Windows;

namespace ADUserSearchTool.Views
{
    public partial class GroupMembershipWindow : Window
    {
        public GroupMembershipWindow()
        {
            InitializeComponent();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtGroups.Text);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}