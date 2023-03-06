using Telegram.Bot;

namespace Gandalf.Processors
{
    public class GhCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public GhCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("gh"))
                return false;

            var cmd = message.Substring(message.IndexOf(' ') + 1).Trim();
            GitService gs = new GitService();
            var res = gs.GhExec(cmd, service.CurrentDir);
            await service.Bot.SendTextMessageAsync(
       chatId: service.ChatId,
       text: "status:\n" + res,
       cancellationToken: service.CancellationToken);

            return true;
        }
    }
}