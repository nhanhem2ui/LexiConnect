using BusinessObjects;
using LexiConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace LexiConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly UserManager<Users> _userManager;
        public AdminController(IGenericRepository<Document> documentRepository, UserManager<Users> userManager)
        {
            _documentRepository = documentRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> AdminManagement()
        {
            var totalDocuments = _documentRepository.GetAllQueryable().Include(d => d.Course);
            var users = _userManager.Users;
            var pendingDocuments = totalDocuments.Where(d => d.Status == "pending" || d.Status == "processing").Include(d => d.Uploader);
            var flaggedContent = totalDocuments.Where(d => d.Status == "flagged");
                
            var model = new AdminManagementViewModel
            {
                RecentUsers = users,
                ActiveSubscriptions = await users.Where(s => s.SubscriptionPlan.Name != "FREE" && s.SubscriptionStartDate.HasValue && s.SubscriptionEndDate.HasValue).CountAsync(),
                TotalUsers = await users.CountAsync(),
                PendingDocuments = await pendingDocuments.CountAsync(),
                FlaggedDocuments = await flaggedContent.CountAsync(),
                FlaggedContent = flaggedContent,
                RecentPendingDocuments = pendingDocuments,
                TotalDocuments = await totalDocuments.CountAsync(),
                TodaysUploads = await totalDocuments.Where(d => d.CreatedAt == DateTime.Today).CountAsync(),
            };

            return View(model);
        }
    }
}
