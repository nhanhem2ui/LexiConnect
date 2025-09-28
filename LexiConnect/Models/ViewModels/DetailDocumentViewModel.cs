using BusinessObjects;
using LexiConnect.Controllers;

namespace LexiConnect.Models.ViewModels
{
    public class DetailDocumentViewModel
    {
        public Document Document { get; set; }
        public UploaderStatsViewModel UploaderStats { get; set; }
        public string FileUrl { get; set; }
        public string FilePDFpath { get; set; }

        public bool CanDownload { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();

        // Helper property to check if viewing PDF version
        public bool IsViewingPdfVersion => !string.IsNullOrEmpty(FilePDFpath) &&
                                          FileUrl == FilePDFpath;

        // Helper property to get display message
        public string ViewerDisplayMessage => IsViewingPdfVersion ?
            (Document.FileType.ToLower() == "pdf" ? "Viewing PDF document" :
             $"Viewing PDF version (converted from {Document.FileType.ToUpper()})") :
            "Viewing original file";
    }
}
