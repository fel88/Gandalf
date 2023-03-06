using Telegram.Bot;

namespace Gandalf.Processors
{
    public class PingCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public PingCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("ping"))
                return false;

            var rep = "I am a servant of the Secret Fire, wielder of the flame of Anor";
            await service.Bot.SendTextMessageAsync(
         chatId: service.ChatId,
         text: rep,
         cancellationToken: service.CancellationToken);
            return true;
        }
    }
}
