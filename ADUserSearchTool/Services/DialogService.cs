using ADUserSearchTool.ViewModels;
using ADUserSearchTool.Views;
using System.Windows;

namespace ADUserSearchTool.Services
{
    public class DialogService : IDialogService
    {
        public void ShowGroupMemberships(string title, string text)
        {
            GroupMembershipWindow window = new GroupMembershipWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = new GroupMembershipViewModel(title, text)
            };

            window.ShowDialog();
        }
    }
}