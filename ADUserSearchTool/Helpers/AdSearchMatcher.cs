using ADUserSearchTool.Enums;
using ADUserSearchTool.Models;

namespace ADUserSearchTool.Helpers
{
    public static class AdSearchMatcher
    {
        public static bool Matches(AdUserResult user, SearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SearchText))
                return true;

            string search = request.SearchText.Trim();
            string searchDigits = AdHelper.NormalizeNumber(request.SearchText);

            switch (request.SearchMode)
            {
                case SearchMode.Phone:
                    return MatchesPhone(user, searchDigits);

                case SearchMode.LogonScript:
                    return AdHelper.ContainsIgnoreCase(user.LogonScript, search);

                case SearchMode.Ou:
                    return AdHelper.ContainsIgnoreCase(user.OU, search) ||
                           AdHelper.ContainsIgnoreCase(user.DistinguishedName, search);

                case SearchMode.Group:
                    return false;

                case SearchMode.All:
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