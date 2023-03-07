using System.Text;
using Telegram.Bot;

namespace Gandalf.Processors
{
    public class ParseCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public ParseCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("parse "))
                return false;

            var spl = messageText.Trim().ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            if (service.Mode == BotMode.File)
            {
                var ss = System.IO.File.ReadAllText(service.CurrentFile);
                var csharpClass = CsharpClassParser.Parse(ss);
                StringBuilder sb = new StringBuilder();

                int start = 0;
                int lines = 0;
                if (spl.Length == 3)
                {
                    start = int.Parse(spl[1]) - 1;
                    lines = int.Parse(spl[2]) - 1;
                }
                else
                    lines = int.Parse(spl[1]) - 1;

                for (int i = start; i < Math.Min(csharpClass.Members.Count, start + lines); i++)
                {
                    var m = csharpClass.Members[i + service.CurrentFileLine];
                    sb.AppendLine(i + $": [{m.Span.Start.Line }-{m.Span.End.Line }] " + csharpClass.Members[i + service.CurrentFileLine]);
                }
                await service.Bot.SendTextMessageAsync(
                        chatId: service.ChatId,
                        text: "total members: " + csharpClass.Members.Count + "\n" + sb.ToString(),
                        cancellationToken: service.CancellationToken);
                return true;
            }
            var add = spl[1];
            var currentDir = service.CurrentDir;
            var cd = new DirectoryInfo(currentDir);
            if (cd.GetFiles().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
            {
                var ss = System.IO.File.ReadAllText(Path.Combine(currentDir, add));
                var csharpClass = CsharpClassParser.Parse(ss);
                StringBuilder sb = new StringBuilder();
                foreach (var cc in csharpClass.Members)
                {
                    sb.AppendLine(cc.Signature );
                }
                var str = sb.ToString();
                if (str.Length > Constants.MessageLengthLimit)
                {
                    if (spl.Length < 3)
                    {
                        str = "please specify number of page 1-" + (str.Length / Constants.MessageLengthLimit + 1);
                    }
                    else
                    {
                        int page = int.Parse(spl[2]) - 1;
                        str = str.Substring(page * Constants.MessageLengthLimit);
                        if (str.Length > Constants.MessageLengthLimit)
                        {
                            str = str.Substring(0, Constants.MessageLengthLimit);
                        }
                    }
                }
                await service.Bot.SendTextMessageAsync(
               chatId: service.ChatId,
               text: str,
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