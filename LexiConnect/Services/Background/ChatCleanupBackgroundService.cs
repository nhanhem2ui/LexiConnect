using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Services;

namespace LexiConnect.Services.Background
{
    public class ChatCleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<ChatCleanupBackgroundService> _logger;
        private readonly IOptionsMonitor<ChatCleanupOptions> _optionsMonitor;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatCleanupBackgroundService(
            ILogger<ChatCleanupBackgroundService> logger,
            IOptionsMonitor<ChatCleanupOptions> optionsMonitor,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupChatsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected during shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while cleaning up chat history");
                }

                var interval = GetInterval();
                _logger.LogDebug("Chat cleanup service sleeping for {Interval}", interval);
                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task CleanupChatsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IGenericService<Chat>>();

            var options = _optionsMonitor.CurrentValue;
            var retentionDays = Math.Max(1, options.RetentionDays);
            var batchSize = Math.Max(50, options.BatchSize);
            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

            var query = chatService
                .GetAllQueryable(c => c.Timestamp < cutoff)
                .OrderBy(chat => chat.Timestamp)
                .Take(batchSize);

            var deleted = await query.ExecuteDeleteAsync(cancellationToken);

            if (deleted == 0)
            {
                _logger.LogDebug("No chats older than {Cutoff} to delete", cutoff);
                return;
            }

            _logger.LogInformation("Deleted {Count} chats older than {Cutoff}", deleted, cutoff);
        }

        private TimeSpan GetInterval()
        {
            var minutes = Math.Max(1, _optionsMonitor.CurrentValue.IntervalMinutes);
            return TimeSpan.FromMinutes(minutes);
        }
    }
}

