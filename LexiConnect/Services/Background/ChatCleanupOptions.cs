namespace LexiConnect.Services.Background
{
    public class ChatCleanupOptions
    {
        public int RetentionDays { get; set; } = 30;
        public int IntervalMinutes { get; set; } = 60;
        public int BatchSize { get; set; } = 200;
    }
}

