using ADUserSearchTool.Enums;
using ADUserSearchTool.Models;
using ADUserSearchTool.Services;
using System.Collections.Generic;

namespace ADUserSearchTool.Tests.Fakes
{
    public class FakeActiveDirectoryService : IActiveDirectoryService
    {
        public List<AdUserResult> UsersToReturn { get; set; } = new List<AdUserResult>();

        public List<AdUserResult> GroupMembersToReturn { get; set; } = new List<AdUserResult>();

        public SearchRequest? LastSearchRequest { get; private set; }

        public string? LastGroupSearchText { get; private set; }

        public UserStatusFilter? LastGroupStatusFilter { get; private set; }

        public List<AdUserResult> SearchUsers(SearchRequest request)
        {
            LastSearchRequest = request;
            return UsersToReturn;
        }

        public List<AdUserResult> SearchGroupMembers(string groupSearchText, UserStatusFilter statusFilter)
        {
            LastGroupSearchText = groupSearchText;
            LastGroupStatusFilter = statusFilter;
            return GroupMembersToReturn;
        }
    }
}