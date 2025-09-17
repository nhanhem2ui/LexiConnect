using LexiConnect.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LexiConnect.Models.ViewModels
{
    public class DocumentDetailsViewModel
    {

        public List<SingleDocumentModel> Documents { get; set; } = new List<SingleDocumentModel>();

        //public IEnumerable<SelectListItem> Courses { get; set; }

            

    }
}
