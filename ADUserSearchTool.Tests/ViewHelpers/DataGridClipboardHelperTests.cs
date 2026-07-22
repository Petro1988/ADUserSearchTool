using ADUserSearchTool.Models;
using ADUserSearchTool.ViewHelpers;
using System.Collections.Generic;
using Xunit;

namespace ADUserSearchTool.Tests.ViewHelpers
{
    public class DataGridClipboardHelperTests
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
                VerbindenMit = @"H: -> \\server\home\p.gavriliuc"
            };
        }

        [Fact]
        public void BuildUserRowText_ReturnsExpectedFields()
        {
            AdUserResult user = CreateUser();

            string result = DataGridClipboardHelper.BuildUserRowText(user);

            Assert.Contains("Name: Petru Gavriliuc", result);
            Assert.Contains("Benutzername: p.gavriliuc", result);
            Assert.Contains("E-Mail: p.gavriliuc@firma.local", result);
            Assert.Contains("Telefon: 123456", result);
            Assert.Contains("Mobile: 987654", result);
            Assert.Contains("Logon Script: hamburg-logon.cmd", result);
            Assert.Contains("Letzte Anmeldung: 2026-07-22 08:00", result);
            Assert.Contains("Letzte Passwortänderung: 2026-07-01 10:00", result);
            Assert.Contains("Status: Aktiv", result);
            Assert.Contains("Kontooptionen: Normales Benutzerkonto", result);
            Assert.Contains("OU: IT / Hamburg", result);
            Assert.Contains(@"Verbinden mit: H: -> \\server\home\p.gavriliuc", result);
            Assert.Contains("Mitglied von: GG_IT; GG_VPN", result);
        }

        [Theory]
        [InlineData("Name", "Petru Gavriliuc")]
        [InlineData("Benutzername", "p.gavriliuc")]
        [InlineData("E-Mail", "p.gavriliuc@firma.local")]
        [InlineData("Telefon", "123456")]
        [InlineData("Mobile", "987654")]
        [InlineData("Logon Script", "hamburg-logon.cmd")]
        [InlineData("Letzte Anmeldung", "2026-07-22 08:00")]
        [InlineData("Letzte Passwortänderung", "2026-07-01 10:00")]
        [InlineData("Mitglied von", "GG_IT; GG_VPN")]
        [InlineData("Status", "Aktiv")]
        [InlineData("Kontooptionen", "Normales Benutzerkonto")]
        [InlineData("OU", "IT / Hamburg")]
        [InlineData("Verbinden mit", @"H: -> \\server\home\p.gavriliuc")]
        public void GetUserValueByHeader_ReturnsExpectedValue(string header, string expected)
        {
            AdUserResult user = CreateUser();

            string result = DataGridClipboardHelper.GetUserValueByHeader(user, header);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetUserValueByHeader_ReturnsEmptyString_ForUnknownHeader()
        {
            AdUserResult user = CreateUser();

            string result = DataGridClipboardHelper.GetUserValueByHeader(user, "Unbekannt");

            Assert.Equal("", result);
        }

        [Fact]
        public void BuildSelectedCellsText_ReturnsSelectedCellValue()
        {
            AdUserResult user = CreateUser();

            List<SelectedCellValue> selectedCells = new List<SelectedCellValue>
            {
                new SelectedCellValue(user, "E-Mail")
            };

            string result = DataGridClipboardHelper.BuildSelectedCellsText(selectedCells);

            Assert.Equal("p.gavriliuc@firma.local", result);
        }

        [Fact]
        public void BuildSelectedCellsText_ReturnsMultipleSelectedCellValues_TabSeparated()
        {
            AdUserResult user = CreateUser();

            List<SelectedCellValue> selectedCells = new List<SelectedCellValue>
            {
                new SelectedCellValue(user, "Name"),
                new SelectedCellValue(user, "E-Mail")
            };

            string result = DataGridClipboardHelper.BuildSelectedCellsText(selectedCells);

            Assert.Equal("Petru Gavriliuc\tp.gavriliuc@firma.local", result);
        }

        [Fact]
        public void BuildSelectedCellsText_ReturnsEmptyString_WhenNoCellsSelected()
        {
            List<SelectedCellValue> selectedCells = new List<SelectedCellValue>();

            string result = DataGridClipboardHelper.BuildSelectedCellsText(selectedCells);

            Assert.Equal("", result);
        }

        [Fact]
        public void BuildSelectedCellsText_ReturnsMultipleRows_OnSeparateLines()
        {
            AdUserResult user1 = CreateUser();

            AdUserResult user2 = new AdUserResult
            {
                Name = "Max Mustermann",
                Email = "max.mustermann@firma.local"
            };

            List<SelectedCellValue> selectedCells = new List<SelectedCellValue>
            {
                new SelectedCellValue(user1, "Name"),
                new SelectedCellValue(user1, "E-Mail"),
                new SelectedCellValue(user2, "Name"),
                new SelectedCellValue(user2, "E-Mail")
            };

            string result = DataGridClipboardHelper.BuildSelectedCellsText(selectedCells);

            Assert.Equal(
                "Petru Gavriliuc\tp.gavriliuc@firma.local\r\nMax Mustermann\tmax.mustermann@firma.local",
                result);
        }
    }
}