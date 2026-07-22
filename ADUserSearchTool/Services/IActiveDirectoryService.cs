using ADUserSearchTool.Enums;
using ADUserSearchTool.Models;
using System.Collections.Generic;

namespace ADUserSearchTool.Services
{
    public interface IActiveDirectoryService
    {
        List<AdUserResult> SearchUsers(SearchRequest request);

        List<AdUserResult> SearchGroupMembers(string groupSearchText, UserStatusFilter statusFilter);
    }
}