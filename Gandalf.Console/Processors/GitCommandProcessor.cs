using Telegram.Bot;

namespace Gandalf.Processors
{
    public class GitCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public GitCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("git"))
                return false;
 
            
                var cmd = message.Substring(message.IndexOf(' ') + 1).Trim();
                GitService gs = new GitService();
                var res = gs.GitExec(cmd, service. CurrentDir);
                 await service.Bot.SendTextMessageAsync(
            chatId: service.ChatId,
            text: "status:\n" + res,
            cancellationToken: service.CancellationToken);
            
            return true;
        }
    }
}