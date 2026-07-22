using ADUserSearchTool.Helpers;
using ADUserSearchTool.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using ADUserSearchTool.Constants;
using ADUserSearchTool.Exceptions;
using ADUserSearchTool.Enums;

namespace ADUserSearchTool.Services
{

    public class ActiveDirectoryService: IActiveDirectoryService
    {
        public List<AdUserResult> SearchUsers(SearchRequest request)
        {
            List<AdUserResult> results = new List<AdUserResult>();

            using DirectoryEntry root = GetDefaultNamingContext();
            using DirectorySearcher searcher = new DirectorySearcher(root);

            searcher.PageSize = 500;
            searcher.SizeLimit = 0;
            searcher.ClientTimeout = TimeSpan.FromSeconds(30);
            searcher.ServerTimeLimit = TimeSpan.FromSeconds(30);

            AddUserProperties(searcher);

            searcher.Filter = "(&(objectCategory=person)(objectClass=user))";

            foreach (SearchResult result in searcher.FindAll())
            {
                AdUserResult user = MapUser(result);

                if (!MatchesStatusFilter(user, request.StatusFilter))
                    continue;

                if (!AdSearchMatcher.Matches(user, request))
                    continue;

                results.Add(user);
            }

            return results
                .OrderBy(x => x.Name)
                .ToList();
        }

        public List<AdUserResult> SearchGroupMembers(string groupSearchText, UserStatusFilter statusFilter)
        {
            List<AdUserResult> members = new List<AdUserResult>();

            if (string.IsNullOrWhiteSpace(groupSearchText))
                return members;

            string safeGroupSearchText = AdHelper.EscapeLdapFilter(groupSearchText.Trim());

            using DirectoryEntry root = GetDefaultNamingContext();
            using DirectorySearcher groupSearcher = new DirectorySearcher(root);

            groupSearcher.PageSize = 500;
            groupSearcher.SizeLimit = 50;
            groupSearcher.ClientTimeout = TimeSpan.FromSeconds(30);
            groupSearcher.ServerTimeLimit = TimeSpan.FromSeconds(30);

            groupSearcher.Filter =
                "(&(objectCategory=group)" +
                "(|" +
                $"(cn=*{safeGroupSearchText}*)" +
                $"(sAMAccountName=*{safeGroupSearchText}*)" +
                "))";

            groupSearcher.PropertiesToLoad.Add("cn");
            groupSearcher.PropertiesToLoad.Add("sAMAccountName");
            groupSearcher.PropertiesToLoad.Add("distinguishedName");
            groupSearcher.PropertiesToLoad.Add("member");

            SearchResultCollection foundGroups = groupSearcher.FindAll();

            if (foundGroups == null || foundGroups.Count == 0)
                return members;

            SearchResult? selectedGroup = null;

            foreach (SearchResult group in foundGroups)
            {
                string cn = GetProperty(group, "cn");
                string sam = GetProperty(group, "sAMAccountName");

                if (string.Equals(cn, groupSearchText, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(sam, groupSearchText, StringComparison.OrdinalIgnoreCase))
                {
                    selectedGroup = group;
                    break;
                }
            }

            if (selectedGroup == null)
            {
                foreach (SearchResult group in foundGroups)
                {
                    if (group.Properties.Contains("member") &&
                        group.Properties["member"].Count > 0)
                    {
                        selectedGroup = group;
                        break;
                    }
                }
            }

            selectedGroup ??= foundGroups[0];

            if (!selectedGroup.Properties.Contains("member") ||
                selectedGroup.Properties["member"].Count == 0)
            {
                return members;
            }

            foreach (object memberObject in selectedGroup.Properties["member"])
            {
                string memberDn = memberObject?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(memberDn))
                    continue;

                try
                {
                    using DirectoryEntry memberEntry = new DirectoryEntry("LDAP://" + memberDn);

                    if (!IsUserObject(memberEntry))
                        continue;

                    AdUserResult user = MapUserFromDirectoryEntry(memberEntry);

                    if (!MatchesStatusFilter(user, statusFilter))
                        continue;

                    members.Add(user);
                }
                catch
                {
                    // Einzelne nicht lesbare Objekte überspringen
                }
            }

            return members
                .OrderBy(x => x.Name)
                .ToList();
        }

        private DirectoryEntry GetDefaultNamingContext()
        {
            try
            {
                using DirectoryEntry rootDse = new DirectoryEntry("LDAP://RootDSE");

                string defaultNamingContext =
                    rootDse.Properties["defaultNamingContext"].Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(defaultNamingContext))
                {
                    throw new InvalidOperationException("defaultNamingContext konnte nicht gelesen werden.");
                }

                return new DirectoryEntry("LDAP://" + defaultNamingContext);
            }
            catch (COMException ex)
            {
                throw new ActiveDirectoryUnavailableException(
                    "Active Directory ist nicht erreichbar.\n\n" +
                    "Bitte prüfen:\n" +
                    "- Bist du im Firmennetzwerk oder per VPN verbunden?\n" +
                    "- Ist die Domäne erreichbar?\n" +
                    "- Funktioniert DNS/Netzwerk?\n\n" +
                    "Technische Meldung:\n" + ex.Message,
                    ex
                );
            }
            catch (Exception ex)
            {
                throw new ActiveDirectoryUnavailableException(
                    "Active Directory konnte nicht initialisiert werden.\n\n" +
                    "Bitte prüfen, ob du im Firmennetzwerk oder per VPN verbunden bist.\n\n" +
                    "Technische Meldung:\n" + ex.Message,
                    ex
                );
            }
        }

        private void AddUserProperties(DirectorySearcher searcher)
        {
            string[] properties =
            {
                "displayName",
                "sAMAccountName",
                "mail",
                "telephoneNumber",
                "mobile",
                "scriptPath",
                "lastLogonTimestamp",
                "pwdLastSet",
                "memberOf",
                "userAccountControl",
                "distinguishedName",
                "homeDrive",
                "homeDirectory"
            };

            foreach (string property in properties)
            {
                searcher.PropertiesToLoad.Add(property);
            }
        }

        private AdUserResult MapUser(SearchResult result)
        {
            string distinguishedName = GetProperty(result, "distinguishedName");
            int userAccountControl = GetIntProperty(result, "userAccountControl");

            return new AdUserResult
            {
                Name = GetProperty(result, "displayName"),
                Benutzername = GetProperty(result, "sAMAccountName"),
                Email = GetProperty(result, "mail"),
                Telefon = GetProperty(result, "telephoneNumber"),
                Mobile = GetProperty(result, "mobile"),
                LogonScript = GetProperty(result, "scriptPath"),
                LetzteAnmeldung = AdHelper.ConvertFileTimeToDate(GetProperty(result, "lastLogonTimestamp")),
                LetztePasswortaenderung = AdHelper.ConvertFileTimeToDate(GetProperty(result, "pwdLastSet")),
                MitgliedVon = GetMultiPropertyCn(result, "memberOf"),
                Status = AdHelper.IsAccountDisabled(userAccountControl) ? "Deaktiviert" : "Aktiv",
                Kontooptionen = BuildAccountOptions(userAccountControl),
                OU = AdHelper.ExtractOu(distinguishedName),
                VerbindenMit = BuildHomeDrive(
                GetProperty(result, "homeDrive"),
                GetProperty(result, "homeDirectory")),
                DistinguishedName = distinguishedName
            };
        }

        private AdUserResult MapUserFromDirectoryEntry(DirectoryEntry entry)
        {
            string distinguishedName = GetEntryProperty(entry, "distinguishedName");
            int userAccountControl = GetEntryIntProperty(entry, "userAccountControl");

            return new AdUserResult
            {
                Name = GetEntryProperty(entry, "displayName"),
                Benutzername = GetEntryProperty(entry, "sAMAccountName"),
                Email = GetEntryProperty(entry, "mail"),
                Telefon = GetEntryProperty(entry, "telephoneNumber"),
                Mobile = GetEntryProperty(entry, "mobile"),
                LogonScript = GetEntryProperty(entry, "scriptPath"),
                LetzteAnmeldung = AdHelper.ConvertFileTimeToDate(GetEntryProperty(entry, "lastLogonTimestamp")),
                LetztePasswortaenderung = AdHelper.ConvertFileTimeToDate(GetEntryProperty(entry, "pwdLastSet")),
                MitgliedVon = GetEntryMultiPropertyCn(entry, "memberOf"),
                Status = AdHelper.IsAccountDisabled(userAccountControl) ? "Deaktiviert" : "Aktiv",
                Kontooptionen = BuildAccountOptions(userAccountControl),
                OU = AdHelper.ExtractOu(distinguishedName),
                VerbindenMit = BuildHomeDrive(
                GetEntryProperty(entry, "homeDrive"),
                GetEntryProperty(entry, "homeDirectory")),
                DistinguishedName = distinguishedName
            };
        }

        private bool MatchesStatusFilter(AdUserResult user, UserStatusFilter statusFilter)
        {
            if (statusFilter == UserStatusFilter.Active)
                return user.Status == "Aktiv";

            if (statusFilter == UserStatusFilter.Disabled)
                return user.Status == "Deaktiviert";

            return true;
        }

        private string GetProperty(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) &&
                result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0]?.ToString() ?? "";
            }

            return "";
        }

        private int GetIntProperty(SearchResult result, string propertyName)
        {
            string value = GetProperty(result, propertyName);

            if (int.TryParse(value, out int number))
                return number;

            return 0;
        }

        private string GetMultiPropertyCn(SearchResult result, string propertyName)
        {
            if (!result.Properties.Contains(propertyName))
                return "";

            List<string> values = new List<string>();

            foreach (object item in result.Properties[propertyName])
            {
                string dn = item.ToString() ?? "";
                string cn = AdHelper.ExtractCn(dn);

                if (!string.IsNullOrWhiteSpace(cn))
                    values.Add(cn);
            }

            return string.Join("; ", values.OrderBy(x => x));
        }

        private bool IsUserObject(DirectoryEntry entry)
        {
            if (!entry.Properties.Contains("objectClass"))
                return false;

            foreach (object value in entry.Properties["objectClass"])
            {
                if (value.ToString()?.Equals("user", StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }

            return false;
        }

        private string GetEntryProperty(DirectoryEntry entry, string propertyName)
        {
            if (entry.Properties.Contains(propertyName) &&
                entry.Properties[propertyName].Count > 0)
            {
                return entry.Properties[propertyName][0]?.ToString() ?? "";
            }

            return "";
        }

        private int GetEntryIntProperty(DirectoryEntry entry, string propertyName)
        {
            string value = GetEntryProperty(entry, propertyName);

            if (int.TryParse(value, out int number))
                return number;

            return 0;
        }

        private string GetEntryMultiPropertyCn(DirectoryEntry entry, string propertyName)
        {
            if (!entry.Properties.Contains(propertyName))
                return "";

            List<string> values = new List<string>();

            foreach (object item in entry.Properties[propertyName])
            {
                string dn = item.ToString() ?? "";
                string cn = AdHelper.ExtractCn(dn);

                if (!string.IsNullOrWhiteSpace(cn))
                    values.Add(cn);
            }

            return string.Join("; ", values.OrderBy(x => x));
        }

        private string BuildHomeDrive(string homeDrive, string homeDirectory)
        {
            if (string.IsNullOrWhiteSpace(homeDrive) &&
                string.IsNullOrWhiteSpace(homeDirectory))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(homeDrive))
                return homeDirectory;

            if (string.IsNullOrWhiteSpace(homeDirectory))
                return homeDrive;

            return $"{homeDrive} -> {homeDirectory}";
        }

        private string BuildAccountOptions(int userAccountControl)
        {
            List<string> options = new List<string>();

            if ((userAccountControl & 2) == 2)
                options.Add("Konto deaktiviert");

            if ((userAccountControl & 16) == 16)
                options.Add("Konto gesperrt");

            if ((userAccountControl & 32) == 32)
                options.Add("Kennwort nicht erforderlich");

            if ((userAccountControl & 64) == 64)
                options.Add("Kennwort kann nicht geändert werden");

            if ((userAccountControl & 512) == 512)
                options.Add("Normales Benutzerkonto");

            if ((userAccountControl & 65536) == 65536)
                options.Add("Kennwort läuft nie ab");

            if ((userAccountControl & 262144) == 262144)
                options.Add("Smartcard erforderlich");

            if ((userAccountControl & 524288) == 524288)
                options.Add("Für Delegierung vertraut");

            if ((userAccountControl & 1048576) == 1048576)
                options.Add("Nicht delegierbar");

            if ((userAccountControl & 2097152) == 2097152)
                options.Add("Nur DES verwenden");

            if ((userAccountControl & 4194304) == 4194304)
                options.Add("Keine Kerberos-Präauthentifizierung");

            if ((userAccountControl & 8388608) == 8388608)
                options.Add("Kennwort abgelaufen");

            if (options.Count == 0)
                return "";

            return string.Join("; ", options);
        }
    }
}