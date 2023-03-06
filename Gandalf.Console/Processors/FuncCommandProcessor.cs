using System.Text;
using Telegram.Bot;

namespace Gandalf.Processors
{
    public class FuncCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public FuncCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("func "))
                return false;

            var spl = messageText.Trim().ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            if (service.Mode != BotMode.File) return false;
            {
                var ss = System.IO.File.ReadAllText(service.CurrentFile);
                var csharpClass = CsharpClassParser.Parse(ss);

                CsSharpMethod method = csharpClass.Methods.First(z => z.Name.ToLower().Contains(spl[1].ToLower()));
                var spl1 = method.Body.Split(new char[] { '\n' }).ToArray();

                StringBuilder sb = new StringBuilder();

                int start = 0;
                int lines = 0;
                if (spl.Length >= 4)
                {
                    start = int.Parse(spl[2]) - 1;
                    lines = int.Parse(spl[3]) - 1;
                }
                else
                    lines = int.Parse(spl[2]) - 1;

                bool noLines = false;
                if (messageText.ToLower().Contains("-nolines"))
                {
                    noLines = true;
                }
                for (int i = start; i < Math.Min(spl1.Length, start + lines); i++)
                {
                    if (!noLines)
                        sb.AppendLine(i + ": " + spl1[i + service.CurrentFileLine]);
                    else
                        sb.AppendLine(spl1[i + service.CurrentFileLine]);
                }
                await service.Bot.SendTextMessageAsync(
                        chatId: service.ChatId,
                        text: "total lines: " + csharpClass.Members.Count,
                        cancellationToken: service.CancellationToken);

                await service.Bot.SendTextMessageAsync(
                        chatId: service.ChatId,
                        text: sb.ToString(),
                        cancellationToken: service.CancellationToken);
                return true;
            }

        }
    }
}