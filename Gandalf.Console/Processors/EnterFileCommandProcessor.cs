using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class EnterFileCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public EnterFileCommandProcessor(ITelegramBotService service) : base(service)
        {

        }

public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("enter "))
                return false;
            var spl = messageText.Trim().ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var add = spl[1];
            var currentDir = service.CurrentDir;
            var cd = new DirectoryInfo(currentDir);
            if (cd.GetFiles().Any(z => z.Name.ToLower(). Contains(add.Trim().ToLower()))) 
            {
                add=cd.GetFiles().First(z => z.Name.ToLower(). Contains(add.Trim().ToLower())).Name;
                service.CurrentFile = Path.Combine(currentDir, add);
                service.Mode = BotMode.File;
                Message sentMessage = await service.Bot.SendTextMessageAsync(
                chatId: service.ChatId,
                text: "file entered",
                cancellationToken: service.CancellationToken);
            }
            else
            {
                await service.Bot.SendTextMessageAsync(
             chatId: service.ChatId,
             text: $"{add} not found!",
             cancellationToken: service.CancellationToken);
            }
            return true;
        }
    }
}
