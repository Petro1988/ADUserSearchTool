using ADUserSearchTool.Models;
using ADUserSearchTool.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ADUserSearchTool
{
    public partial class MainWindow : Window
    {
        private readonly ActiveDirectoryService activeDirectoryService = new ActiveDirectoryService();
        private readonly ExcelExportService excelExportService = new ExcelExportService();

        private string lastSearchText = "";
        private string lastStatusFilter = "Alle";
        private string lastSearchMode = "Alle";

        private List<AdUserResult> currentResults = new List<AdUserResult>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RunSearch();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RunSearch();
                e.Handled = true;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbSearchMode.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;

            dgResults.ItemsSource = null;
            dgResults.SelectedItems.Clear();
            dgResults.SelectedCells.Clear();

            txtStatus.Text = "Bereit.";

            lastSearchText = "";
            lastSearchMode = "Alle";
            lastStatusFilter = "Alle";

            currentResults.Clear();
        }

        private void btnShowGroups_Click(object sender, RoutedEventArgs e)
        {
            ShowSelectedUserGroups();
        }

        private void btnCopyRow_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowToClipboard();
        }

        private void CopySelectedRow_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowToClipboard();
        }

        private void CopyDataGridSelection_Click(object sender, RoutedEventArgs e)
        {
            CopyDataGridSelectionToClipboard();
        }

        private void ShowGroupsFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowSelectedUserGroups();
        }

        private void dgResults_RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow row)
                return;

            if (row.Item is not AdUserResult selectedUser)
                return;

            dgResults.SelectedItems.Clear();
            dgResults.SelectedCells.Clear();

            row.IsSelected = true;
            dgResults.SelectedItem = selectedUser;

            ShowUserGroups(selectedUser);
        }

        private void dgResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject? source = e.OriginalSource as DependencyObject;

            TextBox? textBox = FindParent<TextBox>(source);

            // Wenn in einer TextBox Text markiert wurde, soll markierter Text erhalten bleiben.
            if (textBox != null && !string.IsNullOrEmpty(textBox.SelectedText))
            {
                return;
            }

            DataGridCell? cell = FindParent<DataGridCell>(source);
            DataGridRow? row = FindParent<DataGridRow>(source);

            if (row == null)
                return;

            dgResults.SelectedItems.Clear();
            dgResults.SelectedCells.Clear();

            // Rechtsklick auf echte Zelle:
            // Nur diese Zelle auswählen, NICHT die ganze Zeile.
            if (cell != null && row.Item is AdUserResult cellUser)
            {
                DataGridCellInfo cellInfo = new DataGridCellInfo(cellUser, cell.Column);

                dgResults.CurrentCell = cellInfo;
                dgResults.SelectedCells.Add(cellInfo);

                cell.Focus();

                return;
            }

            // Rechtsklick z.B. auf RowHeader:
            // Dann ganze Zeile auswählen.
            if (row.Item is AdUserResult rowUser)
            {
                row.IsSelected = true;
                dgResults.SelectedItem = rowUser;
                row.Focus();
            }
        }

        private void dgResults_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.C || !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return;

            CopyDataGridSelectionToClipboard();
            e.Handled = true;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (currentResults == null || currentResults.Count == 0)
            {
                MessageBox.Show("Keine Daten zum Exportieren vorhanden.");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Excel Export speichern",
                Filter = "Excel Datei (*.xlsx)|*.xlsx",
                FileName = $"AD_Benutzer_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    excelExportService.ExportToExcel(dialog.FileName, currentResults);
                    MessageBox.Show("Excel Export erfolgreich gespeichert:\n\n" + dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Excel Export:\n\n" + ex.Message);
                }
            }
        }

        private void RunSearch()
        {
            lastSearchText = txtSearch.Text.Trim();
            lastSearchMode = GetSelectedSearchMode();
            lastStatusFilter = GetSelectedStatus();

            SearchAndShowResults(lastSearchText, lastStatusFilter, lastSearchMode);
        }

        private void SearchAndShowResults(string searchText, string statusFilter, string searchMode)
        {
            try
            {
                txtStatus.Text = "Suche läuft...";
                dgResults.ItemsSource = null;
                dgResults.SelectedItems.Clear();
                dgResults.SelectedCells.Clear();

                if (searchMode == "Gruppe")
                {
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        txtStatus.Text = "Bitte Gruppennamen eingeben.";
                        return;
                    }

                    currentResults = activeDirectoryService.SearchGroupMembers(searchText, statusFilter);
                    dgResults.ItemsSource = currentResults;
                    txtStatus.Text = $"{currentResults.Count} Gruppenmitglieder gefunden. Letzte Suche: {DateTime.Now:HH:mm:ss}";
                    return;
                }

                currentResults = activeDirectoryService.SearchUsers(searchText, statusFilter, searchMode);

                if (currentResults.Count == 0 &&
                    !string.IsNullOrWhiteSpace(searchText) &&
                    searchMode == "Alle")
                {
                    txtStatus.Text = "Keine Benutzer gefunden. Suche nach Gruppe...";

                    currentResults = activeDirectoryService.SearchGroupMembers(searchText, statusFilter);
                    dgResults.ItemsSource = currentResults;

                    if (currentResults.Count > 0)
                    {
                        txtStatus.Text = $"{currentResults.Count} Gruppenmitglieder gefunden. Letzte Suche: {DateTime.Now:HH:mm:ss}";
                    }
                    else
                    {
                        txtStatus.Text = "Keine Benutzer oder Gruppe gefunden.";
                    }

                    return;
                }

                dgResults.ItemsSource = currentResults;
                txtStatus.Text = $"{currentResults.Count} Benutzer gefunden. Letzte Suche: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Fehler bei der Suche.";
                MessageBox.Show("Fehler bei der AD-Suche:\n\n" + ex.Message);
            }
        }

        private string GetSelectedStatus()
        {
            if (cmbStatus.SelectedItem is ComboBoxItem item)
                return item.Content?.ToString() ?? "Alle";

            return "Alle";
        }

        private string GetSelectedSearchMode()
        {
            if (cmbSearchMode.SelectedItem is ComboBoxItem item)
                return item.Content?.ToString() ?? "Alle";

            return "Alle";
        }

        private AdUserResult? GetSelectedUser()
        {
            if (dgResults.SelectedItem is AdUserResult selectedUser)
                return selectedUser;

            if (dgResults.SelectedCells.Count > 0 &&
                dgResults.SelectedCells[0].Item is AdUserResult selectedCellUser)
            {
                return selectedCellUser;
            }

            return null;
        }

        private void ShowSelectedUserGroups()
        {
            AdUserResult? selectedUser = GetSelectedUser();

            if (selectedUser == null)
            {
                MessageBox.Show("Bitte zuerst einen Benutzer auswählen.");
                return;
            }

            ShowUserGroups(selectedUser);
        }

        private void ShowUserGroups(AdUserResult selectedUser)
        {
            if (string.IsNullOrWhiteSpace(selectedUser.MitgliedVon))
            {
                MessageBox.Show("Für diesen Benutzer wurden keine Gruppen gefunden.", "Mitglied von");
                return;
            }

            string[] groups = selectedUser.MitgliedVon
                .Split(';')
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .OrderBy(g => g)
                .ToArray();

            string groupText = string.Join(Environment.NewLine, groups);

            ShowLargeTextWindow(
                $"Mitglied von: {selectedUser.Name}",
                groupText
            );
        }

        private void ShowLargeTextWindow(string title, string text)
        {
            Window window = new Window
            {
                Title = title,
                Width = 650,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            Grid grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBox textBox = new TextBox
            {
                Text = text,
                IsReadOnly = true,
                IsReadOnlyCaretVisible = true,
                TextWrapping = TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas")
            };

            Button copyButton = new Button
            {
                Content = "Kopieren",
                Width = 100,
                Height = 32,
                Margin = new Thickness(0, 10, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button closeButton = new Button
            {
                Content = "Schließen",
                Width = 100,
                Height = 32,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(textBox.Text);
                txtStatus.Text = "Mitgliedschaften kopiert.";
            };

            closeButton.Click += (s, e) => window.Close();

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            buttonPanel.Children.Add(copyButton);
            buttonPanel.Children.Add(closeButton);

            Grid.SetRow(textBox, 0);
            Grid.SetRow(buttonPanel, 1);

            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            window.Content = grid;
            window.ShowDialog();
        }

        private void CopySelectedRowToClipboard()
        {
            AdUserResult? selectedUser = GetSelectedUser();

            if (selectedUser == null)
            {
                MessageBox.Show("Bitte zuerst eine Zeile auswählen.");
                return;
            }

            string text = BuildUserRowText(selectedUser);

            Clipboard.SetText(text);
            txtStatus.Text = $"Zeile kopiert: {selectedUser.Name}";
        }

        private void CopyDataGridSelectionToClipboard()
        {
            // Wenn in einer TextBox Text markiert ist, diesen markierten Text kopieren.
            if (Keyboard.FocusedElement is TextBox focusedTextBox &&
                !string.IsNullOrEmpty(focusedTextBox.SelectedText))
            {
                Clipboard.SetText(focusedTextBox.SelectedText);
                txtStatus.Text = "Markierter Text kopiert.";
                return;
            }

            // Wenn Zellen ausgewählt sind, nur diese Zellen kopieren.
            if (dgResults.SelectedCells != null && dgResults.SelectedCells.Count > 0)
            {
                string selectedCellsText = BuildSelectedCellsText();

                if (!string.IsNullOrWhiteSpace(selectedCellsText))
                {
                    Clipboard.SetText(selectedCellsText);
                    txtStatus.Text = "Auswahl kopiert.";
                    return;
                }
            }

            // Fallback: ganze Zeile kopieren.
            CopySelectedRowToClipboard();
        }

        private string BuildSelectedCellsText()
        {
            List<DataGridCellInfo> selectedCells = dgResults.SelectedCells
                .Where(cell => cell.Item is AdUserResult)
                .OrderBy(cell => cell.Column.DisplayIndex)
                .ToList();

            if (selectedCells.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            var rows = selectedCells
                .GroupBy(cell => cell.Item)
                .ToList();

            foreach (var rowGroup in rows)
            {
                AdUserResult user = (AdUserResult)rowGroup.Key;

                List<string> values = new List<string>();

                foreach (DataGridCellInfo cell in rowGroup.OrderBy(cell => cell.Column.DisplayIndex))
                {
                    string header = cell.Column.Header?.ToString() ?? "";
                    string value = GetUserValueByHeader(user, header);

                    values.Add(value);
                }

                sb.AppendLine(string.Join("\t", values));
            }

            return sb.ToString().TrimEnd();
        }

        private string GetUserValueByHeader(AdUserResult user, string header)
        {
            return header switch
            {
                "Name" => user.Name,
                "Benutzername" => user.Benutzername,
                "E-Mail" => user.Email,
                "Telefon" => user.Telefon,
                "Mobile" => user.Mobile,
                "Logon Script" => user.LogonScript,
                "Letzte Anmeldung" => user.LetzteAnmeldung,
                "Mitglied von" => user.MitgliedVon,
                "Status" => user.Status,
                "OU" => user.OU,
                _ => ""
            };
        }

        private string BuildUserRowText(AdUserResult user)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Name: {user.Name}");
            sb.AppendLine($"Benutzername: {user.Benutzername}");
            sb.AppendLine($"E-Mail: {user.Email}");
            sb.AppendLine($"Telefon: {user.Telefon}");
            sb.AppendLine($"Mobile: {user.Mobile}");
            sb.AppendLine($"Logon Script: {user.LogonScript}");
            sb.AppendLine($"Letzte Anmeldung: {user.LetzteAnmeldung}");
            sb.AppendLine($"Status: {user.Status}");
            sb.AppendLine($"OU: {user.OU}");
            sb.AppendLine($"Mitglied von: {user.MitgliedVon}");

            return sb.ToString();
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