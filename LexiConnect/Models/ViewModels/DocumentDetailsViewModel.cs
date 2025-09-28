﻿using LexiConnect.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using static LexiConnect.Controllers.UploadController;
using System.Reflection.Metadata;
namespace LexiConnect.Models.ViewModels
{
    public class DocumentDetailsViewModel
    {

        public List<SingleDocumentModel> Documents { get; set; } = new List<SingleDocumentModel>();

        //public IEnumerable<SelectListItem> Courses { get; set; }

        public IEnumerable<SelectListItem> Courses { get; set; } = new List<SelectListItem>();


      
    }
}
