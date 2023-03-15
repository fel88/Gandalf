using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class InsertCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public InsertCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("insert "))
                return false;
            var spl1 = messageText.Trim().ToLower().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var spl = spl1[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (service.Mode == BotMode.File)
            {
                var ss = System.IO.File.ReadAllLines(Path.Combine(service.CurrentFile));
                int line = int.Parse(spl[1]) - 1;
                string newline = string.Empty;
                if (messageText.IndexOf('\n') != -1)
                    newline = messageText.Substring(messageText.IndexOf('\n') + 1).Replace((char)0xa0, ' ');
                var ss2 = ss.Take(line).Concat(new[] { newline }).Concat(ss.Skip(line)).ToArray();
                System.IO.File.WriteAllLines(service.CurrentFile, ss2);
                await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: "Inserted in line: " + line, cancellationToken: service.CancellationToken);
            }

            return true;
        }
    }
}