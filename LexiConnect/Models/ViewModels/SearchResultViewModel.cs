// SearchResultsViewModel.cs
using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<University> Universities { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
    }
}