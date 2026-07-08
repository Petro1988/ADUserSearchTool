using ADUserSearchTool.Constants;
using ADUserSearchTool.Models;
using ADUserSearchTool.Services;
using ADUserSearchTool.ViewHelpers;
using ADUserSearchTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ADUserSearchTool
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        private AdUserResult? contextMenuUser;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(
                new ActiveDirectoryService(),
                new ExcelExportService(),
                new MessageService(),
                new FileDialogService(),
                new DialogService(),
                new ClipboardTextService()
            );

            Title = AppConstants.WindowTitle;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (ViewModel.SearchCommand.CanExecute(null))
            {
                ViewModel.SearchCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void dgResults_RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow row)
                return;

            if (row.Item is not AdUserResult selectedUser)
                return;

            ViewModel.SelectedUser = selectedUser;

            if (ViewModel.ShowGroupsCommand.CanExecute(null))
            {
                ViewModel.ShowGroupsCommand.Execute(null);
            }
        }

        private void dgResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject? source = e.OriginalSource as DependencyObject;

            TextBox? textBox = FindParent<TextBox>(source);

            if (textBox != null && !string.IsNullOrEmpty(textBox.SelectedText))
                return;

            DataGridCell? cell = FindParent<DataGridCell>(source);
            DataGridRow? row = FindParent<DataGridRow>(source);

            if (row == null)
                return;

            dgResults.SelectedItems.Clear();
            dgResults.SelectedCells.Clear();

            if (row.Item is not AdUserResult user)
                return;

            contextMenuUser = user;

            // Rechtsklick auf eine echte Zelle:
            // Nur diese Zelle auswählen, keine komplette Zeile.
            if (cell != null)
            {
                DataGridCellInfo cellInfo = new DataGridCellInfo(user, cell.Column);

                dgResults.CurrentCell = cellInfo;
                dgResults.SelectedCells.Add(cellInfo);

                cell.Focus();

                e.Handled = false;
                return;
            }

            // Rechtsklick auf RowHeader oder leeren Zeilenbereich:
            // Dann darf die komplette Zeile ausgewählt werden.
            row.IsSelected = true;
            ViewModel.SelectedUser = user;
            row.Focus();
        }

        private void dgResults_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.C || !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return;

            CopyDataGridSelectionToClipboard();
            e.Handled = true;
        }

        private void CopyDataGridSelection_Click(object sender, RoutedEventArgs e)
        {
            CopyDataGridSelectionToClipboard();
        }

        private void CopySelectedRow_Click(object sender, RoutedEventArgs e)
        {
            AdUserResult? user = contextMenuUser ?? ViewModel.SelectedUser;

            if (user != null)
            {
                ViewModel.CopyUserRow(user);
                return;
            }

            if (ViewModel.CopyRowCommand.CanExecute(null))
            {
                ViewModel.CopyRowCommand.Execute(null);
            }
        }

        private void ShowGroupsFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            AdUserResult? user = contextMenuUser ?? ViewModel.SelectedUser;

            if (user != null)
            {
                ViewModel.ShowGroupsForUser(user);
                return;
            }

            if (ViewModel.ShowGroupsCommand.CanExecute(null))
            {
                ViewModel.ShowGroupsCommand.Execute(null);
            }
        }

        private void CopyDataGridSelectionToClipboard()
        {
            if (Keyboard.FocusedElement is TextBox focusedTextBox &&
                !string.IsNullOrEmpty(focusedTextBox.SelectedText))
            {
                Clipboard.SetText(focusedTextBox.SelectedText);
                ViewModel.StatusText = "Markierter Text kopiert.";
                return;
            }

            if (dgResults.SelectedCells != null && dgResults.SelectedCells.Count > 0)
            {
                string selectedCellsText = DataGridClipboardHelper.BuildSelectedCellsText(dgResults);

                if (!string.IsNullOrWhiteSpace(selectedCellsText))
                {
                    Clipboard.SetText(selectedCellsText);
                    ViewModel.StatusText = "Auswahl kopiert.";
                    return;
                }
            }

            if (ViewModel.CopyRowCommand.CanExecute(null))
            {
                ViewModel.CopyRowCommand.Execute(null);
            }
        }

        private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }
    }
}