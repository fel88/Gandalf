using System. Linq;

namespace Gandalf
{
    class Program
    {
        static TelegramBotService bot = new TelegramBotService();        

        static void Main(string[] args)
        { 
            Console.WriteLine("Gandalf wake up..!");
            if(args. Any(z=>z=="--echo")){
            bot. EchoMode=true;
            }
            bot.Run();

            Console.ReadLine();
        }
    }
}
