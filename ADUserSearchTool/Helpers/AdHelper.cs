using System;
using System.Collections.Generic;
using System.Linq;

namespace ADUserSearchTool.Helpers
{
    public static class AdHelper
    {
        public static string EscapeLdapFilter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }

        public static string NormalizeNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return new string(value.Where(char.IsDigit).ToArray());
        }

        public static bool ContainsIgnoreCase(string value, string search)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (string.IsNullOrWhiteSpace(search))
                return true;

            return value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        public static string ExtractCn(string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
                return "";

            string firstPart = distinguishedName.Split(',').FirstOrDefault() ?? "";

            if (firstPart.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                return firstPart.Substring(3);

            return firstPart;
        }

        public static string ExtractOu(string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
                return "";

            List<string> ouParts = distinguishedName
                .Split(',')
                .Where(part => part.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
                .Select(part => part.Substring(3))
                .ToList();

            return string.Join(" / ", ouParts);
        }

        public static bool IsAccountDisabled(int userAccountControl)
        {
            return (userAccountControl & 2) == 2;
        }

        public static string ConvertFileTimeToDate(string fileTimeValue)
        {
            if (!long.TryParse(fileTimeValue, out long fileTime))
                return "";

            if (fileTime <= 0)
                return "";

            try
            {
                return DateTime.FromFileTimeUtc(fileTime)
                    .ToLocalTime()
                    .ToString("yyyy-MM-dd HH:mm");
            }
            catch
            {
                return "";
            }
        }
    }
}