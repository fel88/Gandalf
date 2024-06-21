using System.Diagnostics;
using Telegram.Bot;

namespace Gandalf.Processors
{
    public class BashCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public BashCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("bash "))
                return false;

            var cmd = message.Substring(message.IndexOf(' ') + 1).Trim();
            Process process = new Process();
            process.StartInfo.WorkingDirectory = service.CurrentDir;
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = "-c \"" + cmd + "\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            var res = process.StandardOutput.ReadToEnd();
            var err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(res))
            {
                await service.Bot.SendTextMessageAsync(
           chatId: service.ChatId,
           text: "result:\n" + res,
           cancellationToken: service.CancellationToken);
            }
            if (!string.IsNullOrEmpty(err))
            {
                await service.Bot.SendTextMessageAsync(
           chatId: service.ChatId,
           text: "error:\n" + err,
           cancellationToken: service.CancellationToken);
            }

            return true;
        }
    }
}