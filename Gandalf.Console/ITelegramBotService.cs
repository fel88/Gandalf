using Telegram.Bot;

namespace Gandalf
{
    public interface ITelegramBotService
    {
        string CurrentDir { get; }
        string CurrentFile { get; set; }
        int CurrentFileLine { get; set; }
        ITelegramBotClient Bot { get; }
        long ChatId { get; }
        CancellationToken CancellationToken { get; }
        BotMode Mode { get; set; }
    }
    public enum BotMode
    {
        Dir, File
    }
}
