using ADUserSearchTool.Models;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;

namespace ADUserSearchTool.Services
{
    public class ExcelExportService: IExcelExportService
    {
        public void ExportToExcel(string filePath, List<AdUserResult> users)
        {
            using XLWorkbook workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("AD Benutzer");

            string[] headers =
            {
                "Name",
                "Benutzername",
                "E-Mail",
                "Telefon",
                "Mobile",
                "Logon Script",
                "Letzte Anmeldung",
                "Letzte Passwortänderung",
                "Mitglied von",
                "Status",
                "Kontooptionen",
                "OU",
                "Verbinden mit"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;

            foreach (AdUserResult user in users)
            {
                worksheet.Cell(row, 1).Value = user.Name;
                worksheet.Cell(row, 2).Value = user.Benutzername;
                worksheet.Cell(row, 3).Value = user.Email;
                worksheet.Cell(row, 4).Value = user.Telefon;
                worksheet.Cell(row, 5).Value = user.Mobile;
                worksheet.Cell(row, 6).Value = user.LogonScript;
                worksheet.Cell(row, 7).Value = user.LetzteAnmeldung;
                worksheet.Cell(row, 8).Value = user.LetztePasswortaenderung;
                worksheet.Cell(row, 9).Value = user.MitgliedVon;
                worksheet.Cell(row, 10).Value = user.Status;
                worksheet.Cell(row, 11).Value = user.Kontooptionen;
                worksheet.Cell(row, 12).Value = user.OU;
                worksheet.Cell(row, 14).Value = user.VerbindenMit;

                row++;
            }

            var usedRange = worksheet.RangeUsed();

            if (usedRange != null)
            {
                usedRange.SetAutoFilter();
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }
    }
}