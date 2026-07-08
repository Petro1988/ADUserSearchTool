using ADUserSearchTool.Models;
using System.Collections.Generic;

namespace ADUserSearchTool.Services
{
    public interface IActiveDirectoryService
    {
        List<AdUserResult> SearchUsers(string searchText, string statusFilter, string searchMode);

        List<AdUserResult> SearchGroupMembers(string groupSearchText, string statusFilter);
    }
}