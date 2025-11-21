using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Repositories;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace LexiConnect.Libraries
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IGenericRepository<Chat> _chatRepository;
        private readonly IGenericRepository<Users> _userRepository;

        //all connected users
        public static readonly ConcurrentDictionary<string, ConnectedUser> ConnectedUsers = new();
        
        // Track which chat each user is currently viewing (userId -> receiverId they're chatting with)
        private static readonly ConcurrentDictionary<string, string> UserCurrentChats = new();

        public ChatHub(IGenericRepository<Users> userRepository, IGenericRepository<Chat> chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                //Extracts user ID from claims
                var identity = Context.User?.Identity as ClaimsIdentity;
                var userId = identity?.Claims.FirstOrDefault(c => c.Type == "extension_userId")?.Value
                    ?? identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                //Validates user authentication
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid user authentication");
                    return;
                }

                var userInfo = await _userRepository.GetAsync(u => u.Id == userId);
                if (userInfo == null)
                {
                    await Clients.Caller.SendAsync("Error", "User not found");
                    return;
                }

                // Remove any existing connections for this user
                var existingConnections = ConnectedUsers.Where(kvp => kvp.Value.UserId == userId).ToList();
                foreach (var existing in existingConnections)
                {
                    ConnectedUsers.TryRemove(existing.Key, out _);
                }

                // Creates ConnectedUser object
                var connectedUser = new ConnectedUser
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName ?? "Unknown",
                    PhoneNumber = userInfo.PhoneNumber ?? string.Empty,
                    UserAvatar = userInfo.AvatarUrl ?? string.Empty,
                };

                //Adds to ConnectedUsers dictionary
                ConnectedUsers.TryAdd(Context.ConnectionId, connectedUser);

                // Notify all clients about updated user list
                await Clients.All.SendAsync("UpdatedConnectedUsers", ConnectedUsers.Values.ToList());

                // Welcome the new user
                await Clients.Caller.SendAsync("Connected", "Successfully connected to chat");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Connection error: {ex.Message}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (ConnectedUsers.TryRemove(Context.ConnectionId, out var user))
                {
                    // Remove from current chats tracking
                    UserCurrentChats.TryRemove(user.UserId, out _);
                    
                    await Clients.All.SendAsync("UpdatedConnectedUsers", ConnectedUsers.Values.ToList());
                    await Clients.All.SendAsync("UserDisconnected", user.UserId, user.UserName);
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw
                Console.WriteLine($"Disconnection error: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string message)
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
                {
                    await Clients.Caller.SendAsync("Error", "Sender not found");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message) || message.Length > 500)
                {
                    await Clients.Caller.SendAsync("Error", "Invalid message");
                    return;
                }
                // Save message to database
                var chatMessage = new Chat
                {
                    ChatId = Guid.NewGuid().ToString(),
                    SenderId = sender.UserId,
                    ReceiverId = receiverId,
                    Message = message.Trim(),
                    Timestamp = DateTime.Now
                };

                await _chatRepository.AddAsync(chatMessage);

                Console.WriteLine($"Message saved to DB: {sender.UserName} -> {receiverId}: {message}");

                // Send to receiver if online
                var receiver = ConnectedUsers.Values.FirstOrDefault(u => u.UserId == receiverId);
                if (receiver != null)
                {
                    Console.WriteLine($"Sending message to receiver: {receiver.UserName}");
                    
                    // Check if receiver is currently viewing this chat
                    bool isViewingThisChat = UserCurrentChats.TryGetValue(receiverId, out var currentChat) && 
                                            currentChat == sender.UserId;
                    
                    if (isViewingThisChat)
                    {
                        // Receiver is viewing this chat, send message directly
                        await Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessage",
                            sender.UserId.ToString(), sender.UserName, message, chatMessage.Timestamp.ToString("o"));
                    }
                    else
                    {
                        // Receiver is not viewing this chat, send notification
                        await Clients.Client(receiver.ConnectionId).SendAsync("ReceiveNotification", new
                        {
                            type = "new_message",
                            title = "New Message",
                            message = $"{sender.UserName}: {message}",
                            redirectUrl = $"/Chat/Chat/{sender.UserId}",
                            senderId = sender.UserId,
                            senderName = sender.UserName,
                            timestamp = chatMessage.Timestamp.ToString("o")
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"Receiver not online: {receiverId}");
                }

                // Send confirmation to sender (this tells the sender the message was processed)
                await Clients.Caller.SendAsync("MessageSent", receiverId, message, chatMessage.Timestamp.ToString("o"));

                Console.WriteLine($"Message sent successfully from {sender.UserName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage error: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }
        
        // Method to track which chat the user is currently viewing
        public async Task SetCurrentChat(string receiverId)
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var user))
                {
                    return;
                }
                
                if (string.IsNullOrEmpty(receiverId))
                {
                    UserCurrentChats.TryRemove(user.UserId, out _);
                }
                else
                {
                    UserCurrentChats.AddOrUpdate(user.UserId, receiverId, (key, oldValue) => receiverId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetCurrentChat error: {ex.Message}");
            }
        }

        public async Task UserTyping(string receiverId, bool isTyping)
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
                {
                    return;
                }

                var receiver = ConnectedUsers.Values.FirstOrDefault(u => u.UserId == receiverId);
                if (receiver != null)
                {
                    await Clients.Client(receiver.ConnectionId).SendAsync("UserTyping",
                        sender.UserId.ToString(), sender.UserName, isTyping);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public class ConnectedUser
        {
            public string ConnectionId { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string UserAvatar { get; set; } = string.Empty;
            public DateTime ConnectedAt { get; set; } = DateTime.Now;
        }
    }
}