using BusinessObjects;
using LexiConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Claims;
using static LexiConnect.Models.ChatModel;

namespace LexiConnect.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IGenericService<Users> _userService;
        private readonly IGenericService<Chat> _chatService;

        public ChatController(IGenericService<Users> userService, IGenericService<Chat> chatService)
        {
            _userService = userService;
            _chatService = chatService;
        }

        public async Task<IActionResult> ChatAsync(string id)
        {
            var viewModel = new ChatModel();

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Signin", "Auth");

            viewModel.CurrentUser = await _userService.GetAsync(u => u.Id.Equals(userId));
            if (viewModel.CurrentUser == null) return RedirectToAction("Signin", "Auth");

            await LoadChatUsers(userId, viewModel);

            viewModel.CurrentReceiver = await _userService.GetAsync(u => u.Id.Equals(id));

            await LoadChatHistory(userId, id, viewModel);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChatHistoryAsync(string receiverId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["Error"] = "User not authenticated";
                return RedirectToAction("Homepage", "Home");
            }

            try
            {
                var messages = await _chatService
                    .GetAllQueryable(c => (c.SenderId == userId && c.ReceiverId == receiverId) ||
                                         (c.SenderId == receiverId && c.ReceiverId == userId))
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .OrderBy(c => c.Timestamp)
                    .ToListAsync();

                var chatHistory = messages.Select(m => new
                {
                    chatId = m.ChatId,
                    senderId = m.SenderId,
                    senderName = m.Sender?.UserName ?? "Unknown",
                    senderAvatar = m.Sender?.AvatarUrl,
                    receiverId = m.ReceiverId,
                    receiverName = m.Receiver?.UserName ?? "Unknown",
                    message = m.Message,
                    timestamp = m.Timestamp
                }).ToList();

                return Json(chatHistory);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading chat history: {ex.Message}";
                return RedirectToAction("Homepage, Home");
            }
        }

        private string? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userIdClaim ?? null;
        }

        private async Task LoadChatUsers(string currentUserId, ChatModel viewModel)
        {
            try
            {
                // Get all messages where the user is either sender or receiver
                var userMessages = await _chatService
                    .GetAllQueryable(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .ToListAsync();

                // Group by conversation partner and get the latest message for each
                var conversations = userMessages
                    .GroupBy(c => c.SenderId == currentUserId ? c.ReceiverId : c.SenderId)
                    .Select(g => new
                    {
                        ConversationPartnerId = g.Key,
                        LatestMessage = g.OrderByDescending(c => c.Timestamp).First(),
                        MessageCount = g.Count()
                    })
                    .ToList();

                viewModel.ChatUsers = new List<ChatUserDto>();

                foreach (var conv in conversations.OrderByDescending(c => c.LatestMessage.Timestamp))
                {
                    var partner = conv.LatestMessage.SenderId == currentUserId
                        ? conv.LatestMessage.Receiver
                        : conv.LatestMessage.Sender;

                    if (partner != null)
                    {
                        viewModel.ChatUsers.Add(new ChatUserDto
                        {
                            UserId = partner.Id,
                            UserName = partner.UserName ?? "Unknown",
                            UserAvatar = partner.AvatarUrl,
                            LastMessage = conv.LatestMessage.Message.Length > 50
                                ? string.Concat(conv.LatestMessage.Message.AsSpan(0, 50), "...")
                                : conv.LatestMessage.Message,
                            LastMessageTime = conv.LatestMessage.Timestamp,
                            MessageCount = conv.MessageCount
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chat users: {ex.Message}");
                viewModel.ChatUsers = new List<ChatUserDto>();
            }
        }
        private async Task LoadChatHistory(string currentUserId, string receiverId, ChatModel viewModel)
        {
            try
            {
                viewModel.ChatHistory = await _chatService
                     .GetAllQueryable(c => (c.SenderId == currentUserId && c.ReceiverId == receiverId) ||
                                          (c.SenderId == receiverId && c.ReceiverId == currentUserId))
                     .Include(c => c.Sender)
                     .Include(c => c.Receiver)
                     .OrderBy(c => c.Timestamp)
                     .ToListAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading chat history: {ex.Message}";
                viewModel.ChatHistory = new List<Chat>();
            }
        }
    }
}
