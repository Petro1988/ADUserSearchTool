using ADUserSearchTool.Enums;

namespace ADUserSearchTool.Models
{
    public class SearchRequest
    {
        public string SearchText { get; }

        public SearchMode SearchMode { get; }

        public UserStatusFilter StatusFilter { get; }

        public SearchRequest(
            string searchText,
            SearchMode searchMode,
            UserStatusFilter statusFilter)
        {
            SearchText = searchText?.Trim() ?? "";
            SearchMode = searchMode;
            StatusFilter = statusFilter;
        }
    }
}