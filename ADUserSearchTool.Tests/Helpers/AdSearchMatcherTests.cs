using ADUserSearchTool.Enums;
using ADUserSearchTool.Helpers;
using ADUserSearchTool.Models;
using Xunit;

namespace ADUserSearchTool.Tests.Helpers
{
    public class AdSearchMatcherTests
    {
        private static AdUserResult CreateUser()
        {
            return new AdUserResult
            {
                Name = "Petru Gavriliuc",
                Benutzername = "p.gavriliuc",
                Email = "p.gavriliuc@firma.local",
                Telefon = "+49 40 123456",
                Mobile = "+49 171 987654",
                LogonScript = "hamburg-logon.cmd",
                LetzteAnmeldung = "2026-07-22 08:00",
                LetztePasswortaenderung = "2026-07-01 10:00",
                MitgliedVon = "GG_IT; GG_VPN",
                Status = "Aktiv",
                Kontooptionen = "Normales Benutzerkonto",
                OU = "IT / Hamburg",
                VerbindenMit = "H: -> \\\\server\\home\\p.gavriliuc",
                DistinguishedName = "CN=Petru Gavriliuc,OU=IT,OU=Hamburg,DC=firma,DC=local"
            };
        }

        [Fact]
        public void Matches_All_FindsByName()
        {
            var user = CreateUser();
            var request = new SearchRequest("Petru", SearchMode.All, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_All_FindsByUsername()
        {
            var user = CreateUser();
            var request = new SearchRequest("p.gavriliuc", SearchMode.All, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_All_FindsByEmail()
        {
            var user = CreateUser();
            var request = new SearchRequest("firma.local", SearchMode.All, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_Phone_FindsTelephone()
        {
            var user = CreateUser();
            var request = new SearchRequest("123456", SearchMode.Phone, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_Phone_FindsMobile()
        {
            var user = CreateUser();
            var request = new SearchRequest("987654", SearchMode.Phone, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_LogonScript_FindsScript()
        {
            var user = CreateUser();
            var request = new SearchRequest("hamburg-logon", SearchMode.LogonScript, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_Ou_FindsOu()
        {
            var user = CreateUser();
            var request = new SearchRequest("Hamburg", SearchMode.Ou, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }

        [Fact]
        public void Matches_LogonScript_ReturnsFalse_WhenNoMatch()
        {
            var user = CreateUser();
            var request = new SearchRequest("berlin-logon", SearchMode.LogonScript, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.False(result);
        }

        [Fact]
        public void Matches_EmptySearch_ReturnsTrue()
        {
            var user = CreateUser();
            var request = new SearchRequest("", SearchMode.All, UserStatusFilter.All);

            bool result = AdSearchMatcher.Matches(user, request);

            Assert.True(result);
        }
    }
}