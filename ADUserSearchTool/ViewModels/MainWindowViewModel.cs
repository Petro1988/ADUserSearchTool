using ADUserSearchTool.Commands;
using ADUserSearchTool.Constants;
using ADUserSearchTool.Exceptions;
using ADUserSearchTool.Models;
using ADUserSearchTool.Services;
using ADUserSearchTool.ViewHelpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ADUserSearchTool.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IActiveDirectoryService activeDirectoryService;
        private readonly IExcelExportService excelExportService;
        private readonly IMessageService messageService;
        private readonly IFileDialogService fileDialogService;
        private readonly IDialogService dialogService;
        private readonly IClipboardTextService clipboardTextService;

        private string searchText = "";
        private string selectedSearchMode = SearchModes.All;
        private string selectedStatusFilter = StatusFilters.All;
        private string statusText = "Bereit.";
        private bool isBusy;
        private AdUserResult? selectedUser;

        public ObservableCollection<AdUserResult> Results { get; } = new ObservableCollection<AdUserResult>();

        public string[] SearchModeItems { get; } =
        {
            SearchModes.All,
            SearchModes.Phone,
            SearchModes.LogonScript,
            SearchModes.Ou,
            SearchModes.Group
        };

        public string[] StatusFilterItems { get; } =
        {
            StatusFilters.All,
            StatusFilters.Active,
            StatusFilters.Disabled
        };

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSearchMode
        {
            get => selectedSearchMode;
            set
            {
                selectedSearchMode = value;
                OnPropertyChanged();
            }
        }

        public string SelectedStatusFilter
        {
            get => selectedStatusFilter;
            set
            {
                selectedStatusFilter = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
                RaiseCommandStates();
            }
        }

        public bool IsNotBusy => !IsBusy;

        public AdUserResult? SelectedUser
        {
            get => selectedUser;
            set
            {
                selectedUser = value;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }

        public AsyncRelayCommand SearchCommand { get; }

        public RelayCommand ClearCommand { get; }

        public RelayCommand ShowGroupsCommand { get; }

        public RelayCommand CopyRowCommand { get; }

        public RelayCommand ExportCommand { get; }

        public MainWindowViewModel(
            IActiveDirectoryService activeDirectoryService,
            IExcelExportService excelExportService,
            IMessageService messageService,
            IFileDialogService fileDialogService,
            IDialogService dialogService,
            IClipboardTextService clipboardTextService)
        {
            this.activeDirectoryService = activeDirectoryService;
            this.excelExportService = excelExportService;
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;
            this.clipboardTextService = clipboardTextService;

            SearchCommand = new AsyncRelayCommand(SearchAsync, () => !IsBusy);
            ClearCommand = new RelayCommand(Clear, () => !IsBusy);
            ShowGroupsCommand = new RelayCommand(ShowGroups, () => !IsBusy && SelectedUser != null);
            CopyRowCommand = new RelayCommand(CopySelectedRow, () => !IsBusy && SelectedUser != null);
            ExportCommand = new RelayCommand(Export, () => !IsBusy && Results.Count > 0);
        }

        public async Task SearchAsync()
        {
            try
            {
                IsBusy = true;
                StatusText = "Suche läuft...";
                Results.Clear();
                SelectedUser = null;

                string search = SearchText.Trim();
                string mode = SelectedSearchMode;
                string status = SelectedStatusFilter;

                if (mode == SearchModes.Group)
                {
                    if (string.IsNullOrWhiteSpace(search))
                    {
                        StatusText = "Bitte Gruppennamen eingeben.";
                        return;
                    }

                    var groupResults = await Task.Run(() =>
                        activeDirectoryService.SearchGroupMembers(search, status));

                    SetResults(groupResults);
                    StatusText = $"{Results.Count} Gruppenmitglieder gefunden. Letzte Suche: {DateTime.Now:HH:mm:ss}";
                    return;
                }

                var userResults = await Task.Run(() =>
                    activeDirectoryService.SearchUsers(search, status, mode));

                if (userResults.Count == 0 &&
                    !string.IsNullOrWhiteSpace(search) &&
                    mode == SearchModes.All)
                {
                    StatusText = "Keine Benutzer gefunden. Suche nach Gruppe...";

                    var groupResults = await Task.Run(() =>
                        activeDirectoryService.SearchGroupMembers(search, status));

                    SetResults(groupResults);

                    StatusText = Results.Count > 0
                        ? $"{Results.Count} Gruppenmitglieder gefunden. Letzte Suche: {DateTime.Now:HH:mm:ss}"
                        : "Keine Benutzer oder Gruppe gefunden.";

                    return;
                }

                SetResults(userResults);
                StatusText = $"{Results.Count} Benutzer gefunden. Letzte Suche: {DateTime.Now:HH:mm:ss}";
            }
            catch (ActiveDirectoryUnavailableException ex)
            {
                Results.Clear();
                StatusText = "Active Directory nicht erreichbar.";
                messageService.ShowWarning(ex.Message, "Active Directory nicht erreichbar");
            }
            catch (Exception ex)
            {
                StatusText = "Fehler bei der Suche.";
                messageService.ShowError("Fehler bei der AD-Suche:\n\n" + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SetResults(System.Collections.Generic.List<AdUserResult> users)
        {
            Results.Clear();

            foreach (AdUserResult user in users)
            {
                Results.Add(user);
            }

            RaiseCommandStates();
        }

        private void Clear()
        {
            SearchText = "";
            SelectedSearchMode = SearchModes.All;
            SelectedStatusFilter = StatusFilters.All;
            Results.Clear();
            SelectedUser = null;
            StatusText = "Bereit.";
            RaiseCommandStates();
        }

        private void ShowGroups()
        {
            if (SelectedUser == null)
            {
                messageService.ShowInfo("Bitte zuerst einen Benutzer auswählen.");
                return;
            }

            ShowGroups(SelectedUser);
        }

        private void ShowGroups(AdUserResult user)
        {
            if (string.IsNullOrWhiteSpace(user.MitgliedVon))
            {
                messageService.ShowInfo("Für diesen Benutzer wurden keine Gruppen gefunden.", "Mitglied von");
                return;
            }

            string[] groups = user.MitgliedVon
                .Split(';')
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .OrderBy(g => g)
                .ToArray();

            string groupText = string.Join(Environment.NewLine, groups);

            dialogService.ShowGroupMemberships($"Mitglied von: {user.Name}", groupText);
        }

        private void CopySelectedRow()
        {
            if (SelectedUser == null)
            {
                messageService.ShowInfo("Bitte zuerst eine Zeile auswählen.");
                return;
            }

            CopySelectedRow(SelectedUser);
        }

        private void CopySelectedRow(AdUserResult user)
        {
            string text = DataGridClipboardHelper.BuildUserRowText(user);
            clipboardTextService.SetText(text);

            StatusText = $"Zeile kopiert: {user.Name}";
        }

        private void Export()
        {
            if (Results.Count == 0)
            {
                messageService.ShowInfo("Keine Daten zum Exportieren vorhanden.");
                return;
            }

            string defaultFileName = $"AD_Benutzer_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            string? filePath = fileDialogService.GetExcelSaveFilePath(defaultFileName);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                excelExportService.ExportToExcel(filePath, Results.ToList());
                messageService.ShowInfo("Excel Export erfolgreich gespeichert:\n\n" + filePath);
            }
            catch (Exception ex)
            {
                messageService.ShowError("Fehler beim Excel Export:\n\n" + ex.Message);
            }
        }

        private void RaiseCommandStates()
        {
            SearchCommand.RaiseCanExecuteChanged();
            ClearCommand.RaiseCanExecuteChanged();
            ShowGroupsCommand.RaiseCanExecuteChanged();
            CopyRowCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowGroupsForUser(AdUserResult user)
        {
            ShowGroups(user);
        }

        public void CopyUserRow(AdUserResult user)
        {
            CopySelectedRow(user);
        }
    }
}