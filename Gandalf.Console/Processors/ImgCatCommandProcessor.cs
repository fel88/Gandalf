using Telegram.Bot;

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
            return true;
        }
    }
}
