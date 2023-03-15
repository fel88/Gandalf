using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class CatCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public CatCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("cat "))
                return false;
            var spl = messageText.Trim().ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (service.Mode == BotMode.File)
            {
                var ss = System.IO.File.ReadAllText(Path.Combine(service.CurrentFile));
                var arr = ss.Split(new char[] { '\n' }).ToArray();
                StringBuilder sb = new StringBuilder();
                int start = 0;
                int lines = 0;
                var args = spl.Where(x => !x.Contains("-")).ToArray();
                bool noLines = false;
                if (spl.Any(x => x.ToLower().Contains("--no-lines")))
                    noLines = true;
                if (args.Length == 3)
                {
                    start = int.Parse(spl[1]) - 1;
                    lines = int.Parse(spl[2]);
                }
                else
                    lines = int.Parse(spl[1]);
                for (int i = start; i < start + lines; i++)
                {
                    if (noLines)
                        sb.AppendLine(arr[i + service.CurrentFileLine]);
                    else
                        sb.AppendLine((i + 1) + ": " + arr[i + service.CurrentFileLine]);
                }

                await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: "total lines: " + arr.Length, cancellationToken: service.CancellationToken);
                await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: sb.ToString(), cancellationToken: service.CancellationToken);
            }
            else
            {
                var add = spl[1];
                var currentDir = service.CurrentDir;
                var cd = new DirectoryInfo(currentDir);
                if (cd.GetFiles().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
                {
                    var ss = System.IO.File.ReadAllText(Path.Combine(currentDir, add));
                    if (ss.Length > Constants.MessageLengthLimit)
                    {
                        if (spl.Length < 3)
                        {
                            Message sentMessage = await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: "please specify number of page 1-" + (ss.Length / Constants.MessageLengthLimit + 1), cancellationToken: service.CancellationToken);
                        }
                        else
                        {
                            int page = int.Parse(spl[2]) - 1;
                            var sub = ss.Substring(page * Constants.MessageLengthLimit);
                            if (sub.Length > Constants.MessageLengthLimit)
                            {
                                sub = sub.Substring(0, Constants.MessageLengthLimit);
                            }

                            await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: sub, cancellationToken: service.CancellationToken);
                        }
                    }
                    else
                    {
                        Message sentMessage = await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: ss, cancellationToken: service.CancellationToken);
                    }
                }
                else
                {
                    await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: $"{add} not found!", cancellationToken: service.CancellationToken);
                }
            }

            return true;
        }
    }
}