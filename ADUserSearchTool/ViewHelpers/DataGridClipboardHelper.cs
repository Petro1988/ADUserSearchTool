using ADUserSearchTool.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ADUserSearchTool.ViewHelpers
{
    public static class DataGridClipboardHelper
    {
        public static string BuildSelectedCellsText(DataGrid dataGrid)
        {
            List<SelectedCellValue> selectedCells = dataGrid.SelectedCells
                .Where(cell => cell.Item is AdUserResult)
                .OrderBy(cell => cell.Column.DisplayIndex)
                .Select(cell =>
                {
                    AdUserResult user = (AdUserResult)cell.Item;
                    string header = cell.Column.Header?.ToString() ?? "";

                    return new SelectedCellValue(user, header);
                })
                .ToList();

            return BuildSelectedCellsText(selectedCells);
        }

        public static string BuildSelectedCellsText(IEnumerable<SelectedCellValue> selectedCells)
        {
            List<SelectedCellValue> cells = selectedCells.ToList();

            if (cells.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            var rowGroups = cells
                .GroupBy(cell => cell.User)
                .ToList();

            foreach (var rowGroup in rowGroups)
            {
                List<string> values = new List<string>();

                foreach (SelectedCellValue cell in rowGroup)
                {
                    string value = GetUserValueByHeader(cell.User, cell.Header);
                    values.Add(value);
                }

                sb.AppendLine(string.Join("\t", values));
            }

            return sb.ToString().TrimEnd();
        }

        public static string BuildUserRowText(AdUserResult user)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Name: {user.Name}");
            sb.AppendLine($"Benutzername: {user.Benutzername}");
            sb.AppendLine($"E-Mail: {user.Email}");
            sb.AppendLine($"Telefon: {user.Telefon}");
            sb.AppendLine($"Mobile: {user.Mobile}");
            sb.AppendLine($"Logon Script: {user.LogonScript}");
            sb.AppendLine($"Letzte Anmeldung: {user.LetzteAnmeldung}");
            sb.AppendLine($"Letzte Passwortänderung: {user.LetztePasswortaenderung}");
            sb.AppendLine($"Status: {user.Status}");
            sb.AppendLine($"Kontooptionen: {user.Kontooptionen}");
            sb.AppendLine($"OU: {user.OU}");
            sb.AppendLine($"Verbinden mit: {user.VerbindenMit}");
            sb.AppendLine($"Mitglied von: {user.MitgliedVon}");

            return sb.ToString();
        }

        public static string GetUserValueByHeader(AdUserResult user, string header)
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
                "Letzte Passwortänderung" => user.LetztePasswortaenderung,
                "Mitglied von" => user.MitgliedVon,
                "Status" => user.Status,
                "Kontooptionen" => user.Kontooptionen,
                "OU" => user.OU,
                "Verbinden mit" => user.VerbindenMit,
                _ => ""
            };
        }
    }

    public class SelectedCellValue
    {
        public AdUserResult User { get; }

        public string Header { get; }

        public SelectedCellValue(AdUserResult user, string header)
        {
            User = user;
            Header = header;
        }
    }
}