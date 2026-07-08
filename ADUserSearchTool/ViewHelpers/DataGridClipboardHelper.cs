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
            List<DataGridCellInfo> selectedCells = dataGrid.SelectedCells
                .Where(cell => cell.Item is AdUserResult)
                .OrderBy(cell => cell.Column.DisplayIndex)
                .ToList();

            if (selectedCells.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            var rowGroups = selectedCells
                .GroupBy(cell => cell.Item)
                .ToList();

            foreach (var rowGroup in rowGroups)
            {
                if (rowGroup.Key is not AdUserResult user)
                    continue;

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
            sb.AppendLine($"Status: {user.Status}");
            sb.AppendLine($"OU: {user.OU}");
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
                "Mitglied von" => user.MitgliedVon,
                "Status" => user.Status,
                "OU" => user.OU,
                _ => ""
            };
        }
    }
}