using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class ExitFileCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public ExitFileCommandProcessor(ITelegramBotService service) : base(service)
        {

        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("exit"))
                return false;
            service.Mode = BotMode.Dir;



            service.CurrentFile = "";

            Message sentMessage = await service.Bot.SendTextMessageAsync(
            chatId: service.ChatId,
            text: "dir mode",
            cancellationToken: service.CancellationToken);


            return true;
        }
    }
}