using ADUserSearchTool.Enums;
using ADUserSearchTool.Models;
using ADUserSearchTool.Tests.Fakes;
using ADUserSearchTool.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ADUserSearchTool.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private static AdUserResult CreateUser()
        {
            return new AdUserResult
            {
                Name = "Petru Gavriliuc",
                Benutzername = "p.gavriliuc",
                Email = "p.gavriliuc@firma.local",
                Telefon = "123456",
                Mobile = "987654",
                LogonScript = "hamburg-logon.cmd",
                LetzteAnmeldung = "2026-07-22 08:00",
                LetztePasswortaenderung = "2026-07-01 10:00",
                MitgliedVon = "GG_IT; GG_VPN",
                Status = "Aktiv",
                Kontooptionen = "Normales Benutzerkonto",
                OU = "IT / Hamburg",
                VerbindenMit = "H: -> \\\\server\\home\\p.gavriliuc"
            };
        }

        private static MainWindowViewModel CreateViewModel(
            FakeActiveDirectoryService adService,
            FakeExcelExportService? excelService = null,
            FakeMessageService? messageService = null,
            FakeFileDialogService? fileDialogService = null,
            FakeDialogService? dialogService = null,
            FakeClipboardTextService? clipboardService = null)
        {
            return new MainWindowViewModel(
                adService,
                excelService ?? new FakeExcelExportService(),
                messageService ?? new FakeMessageService(),
                fileDialogService ?? new FakeFileDialogService(),
                dialogService ?? new FakeDialogService(),
                clipboardService ?? new FakeClipboardTextService()
            );
        }

        [Fact]
        public async Task SearchCommand_SearchesUsers_AndFillsResults()
        {
            var adService = new FakeActiveDirectoryService();
            adService.UsersToReturn.Add(CreateUser());

            var viewModel = CreateViewModel(adService);
            viewModel.SearchText = "Petru";

            await viewModel.SearchAsync();

            Assert.Single(viewModel.Results);
            Assert.Equal("Petru Gavriliuc", viewModel.Results[0].Name);
            Assert.Contains("Benutzer gefunden", viewModel.StatusText);
        }

        [Fact]
        public async Task SearchAsync_GroupMode_SearchesGroupMembers()
        {
            var adService = new FakeActiveDirectoryService();
            adService.GroupMembersToReturn.Add(CreateUser());

            var viewModel = CreateViewModel(adService);
            viewModel.SearchText = "GG_IT";
            viewModel.SelectedSearchMode = viewModel.SearchModeItems.First(x => x.Value == SearchMode.Group);

            await viewModel.SearchAsync();

            Assert.Single(viewModel.Results);
            Assert.Equal("GG_IT", adService.LastGroupSearchText);
            Assert.Contains("Gruppenmitglieder gefunden", viewModel.StatusText);
        }

        [Fact]
        public void ClearCommand_ClearsState()
        {
            var adService = new FakeActiveDirectoryService();
            var viewModel = CreateViewModel(adService);

            viewModel.SearchText = "abc";
            viewModel.Results.Add(CreateUser());
            viewModel.SelectedUser = viewModel.Results.First();

            viewModel.ClearCommand.Execute(null);

            Assert.Equal("", viewModel.SearchText);
            Assert.Empty(viewModel.Results);
            Assert.Null(viewModel.SelectedUser);
            Assert.Equal("Bereit.", viewModel.StatusText);
        }

        [Fact]
        public void CopyRowCommand_CopiesSelectedUser()
        {
            var adService = new FakeActiveDirectoryService();
            var clipboardService = new FakeClipboardTextService();

            var viewModel = CreateViewModel(adService, clipboardService: clipboardService);
            var user = CreateUser();

            viewModel.Results.Add(user);
            viewModel.SelectedUser = user;

            viewModel.CopyRowCommand.Execute(null);

            Assert.NotNull(clipboardService.LastText);
            Assert.Contains("Petru Gavriliuc", clipboardService.LastText);
            Assert.Contains("p.gavriliuc", clipboardService.LastText);
        }

        [Fact]
        public void ShowGroupsCommand_OpensDialog()
        {
            var adService = new FakeActiveDirectoryService();
            var dialogService = new FakeDialogService();

            var viewModel = CreateViewModel(adService, dialogService: dialogService);
            var user = CreateUser();

            viewModel.Results.Add(user);
            viewModel.SelectedUser = user;

            viewModel.ShowGroupsCommand.Execute(null);

            Assert.True(dialogService.WasCalled);
            Assert.Contains("Mitglied von", dialogService.LastTitle);
            Assert.Contains("GG_IT", dialogService.LastText);
        }

        [Fact]
        public void ExportCommand_ExportsResults()
        {
            var adService = new FakeActiveDirectoryService();
            var excelService = new FakeExcelExportService();
            var fileDialogService = new FakeFileDialogService
            {
                FilePathToReturn = "test.xlsx"
            };

            var viewModel = CreateViewModel(
                adService,
                excelService: excelService,
                fileDialogService: fileDialogService);

            viewModel.Results.Add(CreateUser());

            viewModel.ExportCommand.Execute(null);

            Assert.True(excelService.WasCalled);
            Assert.Equal("test.xlsx", excelService.LastFilePath);
            Assert.Single(excelService.LastUsers);
        }
    }
}