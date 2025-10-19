using BusinessObjects;

namespace LexiConnect.Models
{
    public class ChatModel
    {
        public Users? CurrentUser { get; set; }
        public List<Users> ConnectedUsers { get; set; } = new();
        public Users? CurrentReceiver { get; set; }
        public List<ChatUserDto> ChatUsers { get; set; } = new();
        public List<Chat> ChatHistory { get; set; } = new();

        public class ChatUserDto
        {
            public string UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string? UserAvatar { get; set; }
            public string LastMessage { get; set; } = string.Empty;
            public DateTime LastMessageTime { get; set; }
            public int MessageCount { get; set; }
        }
    }
}
