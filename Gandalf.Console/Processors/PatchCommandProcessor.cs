using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class PatchCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public PatchCommandProcessor(ITelegramBotService service) : base(service)
        {

        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("patch "))
                return false;

            var spl = messageText.Replace((char)0xA0,' ') .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var spl1 = spl[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            if (service.Mode == BotMode.File)
            {
                //replace func with specific patch
                var lns = System.IO.File.ReadAllLines(service.CurrentFile);
                var cls = CsharpClassParser.Parse(System.IO.File.ReadAllText(service.CurrentFile));
                var mth = cls.Methods.First(z => z.Name.ToLower().Contains(spl1[1].ToLower()));

                List<string> lines = new List<string>();
                for (int i = 0; i < mth.Span.Start.Line; i++)
                {
                    lines.Add(lns[i]);
                }
                lines.AddRange(spl.Skip(1).ToArray());
                for (int i = mth.Span.End.Line + 1; i < lns.Length; i++)
                {
                    lines.Add(lns[i]);
                }
                System.IO.File.WriteAllLines(service.CurrentFile, lines.ToArray());

                await service.Bot.SendTextMessageAsync(
                chatId: service.ChatId,
        text: $"func  {mth.Name} was patched",
        cancellationToken: service.CancellationToken);
                return true;
            }
            var add = spl[0].Substring(messageText.IndexOf(' ') + 1).Trim().ToLower();

            var code = messageText.Substring(messageText.IndexOf('\n') + 1).Trim();
            var cd = new DirectoryInfo(service.CurrentDir);
            if (cd.GetFiles().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
            {
                var path = Path.Combine(service.CurrentDir, add);

                bool format = true;
                if (format)
                    code = CSharpSyntaxTree.ParseText(code).GetRoot()
                        .NormalizeWhitespace().SyntaxTree.GetText().ToString();

                System.IO.File.WriteAllText(path, code);


                await service.Bot.SendTextMessageAsync(
                chatId: service.ChatId,
        text: $"file {add} was patched",
        cancellationToken: service.CancellationToken);
            }
            else
            {
                await service.Bot.SendTextMessageAsync(
              chatId: service.ChatId,
         text: $"{add} file not found ",
         cancellationToken: service.CancellationToken);



            }
            return true;
        }
    }
}