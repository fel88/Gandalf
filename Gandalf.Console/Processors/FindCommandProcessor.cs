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
            return true;
        }
    }
}
