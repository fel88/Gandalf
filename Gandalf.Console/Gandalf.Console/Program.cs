namespace Gandalf
{
    class Program
    {
        static TelegramBotService bot = new TelegramBotService();        

        static void Main(string[] args)
        { 
            Console.WriteLine("Gandalf wake up..!");
            bot.Run();

            Console.ReadLine();
        }
    }
}
