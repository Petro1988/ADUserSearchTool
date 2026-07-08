using ADUserSearchTool.Constants;
using ADUserSearchTool.Models;

namespace ADUserSearchTool.Helpers
{
    public static class AdSearchMatcher
    {
        public static bool Matches(AdUserResult user, string searchText, string searchMode)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            string search = searchText.Trim();
            string searchDigits = AdHelper.NormalizeNumber(searchText);

            switch (searchMode)
            {
                case SearchModes.Phone:
                    return MatchesPhone(user, searchDigits);

                case SearchModes.LogonScript:
                    return AdHelper.ContainsIgnoreCase(user.LogonScript, search);

                case SearchModes.Ou:
                    return AdHelper.ContainsIgnoreCase(user.OU, search) ||
                           AdHelper.ContainsIgnoreCase(user.DistinguishedName, search);

                case SearchModes.All:
                default:
                    bool textMatch =
                        AdHelper.ContainsIgnoreCase(user.Name, search) ||
                        AdHelper.ContainsIgnoreCase(user.Benutzername, search) ||
                        AdHelper.ContainsIgnoreCase(user.Email, search) ||
                        AdHelper.ContainsIgnoreCase(user.LogonScript, search) ||
                        AdHelper.ContainsIgnoreCase(user.OU, search) ||
                        AdHelper.ContainsIgnoreCase(user.DistinguishedName, search);

                    bool phoneMatch = MatchesPhone(user, searchDigits);

                    return textMatch || phoneMatch;
            }
        }

        private static bool MatchesPhone(AdUserResult user, string searchDigits)
        {
            if (string.IsNullOrWhiteSpace(searchDigits))
                return false;

            string telefonDigits = AdHelper.NormalizeNumber(user.Telefon);
            string mobileDigits = AdHelper.NormalizeNumber(user.Mobile);

            return telefonDigits.Contains(searchDigits) ||
                   mobileDigits.Contains(searchDigits);
        }
    }
}