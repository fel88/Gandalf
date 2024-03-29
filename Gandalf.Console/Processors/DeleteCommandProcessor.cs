using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Gandalf.Processors
{
    public class DeleteCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public DeleteCommandProcessor(ITelegramBotService service) : base(service)
        {

        }

        public async Task<bool> Process(string messageText)
        {
            if (!messageText.Trim().ToLower().StartsWith("delete "))
                return false;

            var spl1 = messageText.Trim().ToLower().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var spl = spl1[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (service.Mode == BotMode.File)
            {
                var ss = System.IO.File.ReadAllLines(Path.Combine(service.CurrentFile));
                //var arr = ss. Split(new char[] { '\n' }).ToArray();
                
        int line = int.Parse(spl[1])-1;
                
                var ss2 = ss.Take(line).Concat(ss.Skip(line+1)).ToArray();
                System.IO.File.WriteAllLines(service.CurrentFile, ss2);


                await service.Bot.SendTextMessageAsync(
                        chatId: service.ChatId,
                        text: "Deleted line: " + line,
                        cancellationToken: service.CancellationToken);

            }

            return true;
        }
    }
}
