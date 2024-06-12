using SixLabors.ImageSharp;

namespace Gandalf
{
    class Program
    {
        static TelegramBotService bot = new TelegramBotService();
        public static DateTime StartTimestamp = DateTime.Now;
        static void Main(string[] args)
        {
            Console.WriteLine("Gandalf wake up..!");
            if (args.Any(z => z == "--echo"))
            {
                bot.EchoMode = true;
            }            
            bot.Run();
            Console.ReadLine();
        }
    }
}
