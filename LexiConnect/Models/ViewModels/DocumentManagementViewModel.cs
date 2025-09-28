using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class DocumentManagementViewModel
    {
        public IEnumerable<Document> Documents { get; set; } = new List<Document>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;

        // Filter and search properties
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? TypeFilter { get; set; }
        public string? CourseFilter { get; set; }
        public string? UploaderFilter { get; set; }

        // Sorting properties
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";

        //Document statistics
        public int TotalDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int FlaggedDocuments { get; set; }
    }
}