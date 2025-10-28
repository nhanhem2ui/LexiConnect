using BusinessObjects;

namespace LexiConnect.Models.ViewModels
{
    public class UniversitiesViewModel
    {
        public List<University> Universities { get; set; } = new();
        public List<University> PopularUniversities { get; set; } = new();
        public string SearchQuery { get; set; } = "";
        public string SelectedLetter { get; set; } = "All";
    }
}