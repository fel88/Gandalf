using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using Telegram.Bot;

namespace Gandalf.Processors
{
    public class FindCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public FindCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("find "))
                return false;

            var spl = message.ToLower().Split(new char[] { ' ' }).ToArray();
            if (service.Mode == BotMode.File)
            {
                StringBuilder sb = new StringBuilder();
                var ln = File.ReadAllLines(service.CurrentFile);
                for (int i = 0; i < ln.Length; i++)
                {
                    if (ln[i].ToLower().Contains(spl[1]))
                        sb.AppendLine($"{i}: " + ln[i]);
                }
                await service.Bot.SendTextMessageAsync(
            chatId: service.ChatId,
            text: "matches: " + sb.Length + "\n" + sb.ToString(),
            cancellationToken: service.CancellationToken);
            }
            else
            {
                var cd = new DirectoryInfo(service.CurrentDir);

                foreach (var item in cd.GetFiles())
                {
                    StringBuilder sb = new StringBuilder();
                    var ln = File.ReadAllLines(item.FullName);
                    int matches = 0;
                    for (int i = 0; i < ln.Length; i++)
                    {
                        if (ln[i].ToLower().Contains(spl[1]))
                        {
                            sb.AppendLine($"{i + 1}: " + ln[i]);
                            matches++;
                        }
                    }
                    if (matches == 0)
                        continue;

                    await service.Bot.SendTextMessageAsync(
                chatId: service.ChatId,
                text: $"{item.Name}\nmatches: {matches}\n{sb}",
                cancellationToken: service.CancellationToken);
                }

            }

            return true;
        }
    }
}
