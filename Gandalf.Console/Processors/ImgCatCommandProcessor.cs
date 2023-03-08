using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace Gandalf.Processors
{
    public class ImgCatCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public ImgCatCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("imgcat "))
                return false;

            var add = message.Substring(message.IndexOf(' ') + 1).Trim().ToLower();

            var cd = new DirectoryInfo(service.CurrentDir);
            if (cd.GetFiles().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
            {
                var path = Path.Combine(service.CurrentDir, add);
                using (var r = System.IO.File.OpenRead(path))
                {
                    InputOnlineFile file = new InputOnlineFile(r, Path.GetFileName(path));

                    await service.Bot.SendPhotoAsync(
                    chatId: service.ChatId,
                    photo: file,
                    cancellationToken: service.CancellationToken);
                }
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
