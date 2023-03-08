using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class FormatCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public FormatCommandProcessor(ITelegramBotService service) : base(service)
        {

        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("format"))
                return false;

            if (service.Mode == BotMode.File)
            {
                var code = System.IO.File.ReadAllText(service.CurrentFile);
                bool format = true;
                if (format)
                    code = CSharpSyntaxTree.ParseText(code).GetRoot()
                        .NormalizeWhitespace().SyntaxTree.GetText().ToString();

                System.IO.File.WriteAllText(service.CurrentFile, code);

                await service.Bot.SendTextMessageAsync(
          chatId: service.ChatId,
     text: $"{service.CurrentFile} formatted ",
     cancellationToken: service.CancellationToken);
            }
            return true;
        }
    }
}