using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using System.Text;
using LibGit2Sharp;
using System.Xml.Linq;
using Telegram.Bot.Types.InputFiles;

namespace Gandalf
{
    public class TelegramBotService
    {
        TelegramBotClient botClient;

        public void LoadConfig()
        {
            var doc = XDocument.Load(configFileName);
            foreach (var item in doc.Descendants("setting"))
            {
                var nm = item.Attribute("name").Value;
                var vl = item.Attribute("value").Value;
                switch (nm)
                {
                    case "apiKey":
                        apiKey = vl;
                        break;
                    case "repsDir":
                        repositoriesFolder = vl;
                        break;
                    case "chatId":
                        targetChatId = long.Parse(vl);
                        break;
                    case "msbuildPath":
                        msbuildPath = vl;
                        break;
                }
            }
        }

        const string configFileName = "config.xml";
        string apiKey;
        long targetChatId;
        string repositoriesFolder;
        string msbuildPath;

        public async void Run()
        {
            if (!System.IO.File.Exists(configFileName))
            {
                Console.WriteLine($"{configFileName} not found. You shall not pass!");
                return;
            }
            LoadConfig();
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("apiKey is empty. You shall not pass!");
                return;
            }
            if (!Directory.Exists(repositoriesFolder))
            {
                Console.WriteLine($"{repositoriesFolder} doestn't exist. You shall not pass!");
                return;
            }
            currentDir = repositoriesFolder;
            botClient = new TelegramBotClient(apiKey);

            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();
        }

        string currentDir = "c:\\git";
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            if (chatId != targetChatId)
            {
                Console.WriteLine("unauthorized access from chatId: " + chatId);
                return;
            }
            messageText = messageText.ToLower().Trim();
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            if (messageText.ToLower().StartsWith("ls"))
            {


                StringBuilder sb = new StringBuilder();
                var d = new DirectoryInfo(currentDir);
                currentDir = d.FullName;
                foreach (var item in d.GetDirectories())
                {
                    sb.AppendLine(item.Name);
                }
                foreach (var item in d.GetFiles())
                {
                    sb.AppendLine(item.Name);
                }
                Message sentMessage = await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: sb.ToString(),
              cancellationToken: cancellationToken);
            }
            else if (messageText.StartsWith("build"))
            {
                try
                {
                    MsbuildService ms = new MsbuildService();
                    ms.MsBuildPath = msbuildPath;
                    GitService gs = new GitService();
                    var res = ms.Build(new RepositoryInfo() { Path = currentDir }, gs);

                    Message sentMessage = await botClient.SendTextMessageAsync(
          chatId: chatId,
          text: res.ToString(),
          cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    Message sentMessage = await botClient.SendTextMessageAsync(
          chatId: chatId,
          text: ex.Message,
          cancellationToken: cancellationToken);
                }
            }
            else if (messageText.StartsWith("git status"))
            {
                using (var repo = new Repository(currentDir))
                {
                    Commit commit = repo.Head.Tip;
                    Console.WriteLine("Author: {0}", commit.Author.Name);
                    Console.WriteLine("Message: {0}", commit.MessageShort);
                    var status = repo.RetrieveStatus();

                    Tree commitTree = repo.Head.Tip.Tree;
                    Tree parentCommitTree = repo.Head.Tip.Parents.Single().Tree;

                    //var patch = repo.Diff.Compare<Patch>(commitTree, parentCommitTree);
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in status)
                    {
                        sb.AppendLine($"{item.FilePath} {item.State}");
                    }

                    /*sb.AppendLine(string.Format("{0} files changed.", patch.Count()));

                    foreach (var pec in patch)
                    {
                        sb.AppendLine(string.Format("{0} = {1} ({2}+ and {3}-)",
                            pec.Path,
                            pec.LinesAdded + pec.LinesDeleted,
                            pec.LinesAdded,
                            pec.LinesDeleted));
                    }*/
                    Message sentMessage = await botClient.SendTextMessageAsync(
             chatId: chatId,
             text: sb.ToString(),
             cancellationToken: cancellationToken);

                }
            }
            else if (messageText.StartsWith("cd.."))
            {
                var cd = new DirectoryInfo(currentDir);
                currentDir = cd.Parent.FullName;
                if (!new DirectoryInfo(currentDir).FullName.ToLower().StartsWith(new DirectoryInfo(repositoriesFolder).FullName.ToLower()))
                {
                    currentDir = repositoriesFolder;
                }
                Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "current dir: " + currentDir,
            cancellationToken: cancellationToken);
            }
            else if (messageText.StartsWith("cd"))
            {
                if (messageText.Contains(".."))
                {
                    var cd = new DirectoryInfo(currentDir);
                    currentDir = cd.Parent.FullName;
                    if (!new DirectoryInfo(currentDir).FullName.ToLower().StartsWith(new DirectoryInfo(repositoriesFolder).FullName.ToLower()))
                    {
                        Console.WriteLine("reset current folder");
                        currentDir = repositoriesFolder;
                    }                    
                }
                else
                if (messageText.Contains(' '))
                {
                    var add = messageText.Substring(messageText.IndexOf(' ') + 1);
                    var dd = new DirectoryInfo(currentDir);
                    if (dd.GetDirectories().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
                    {
                        currentDir = Path.Combine(currentDir, add);
                        if (!new DirectoryInfo(currentDir).FullName.ToLower().StartsWith(new DirectoryInfo(repositoriesFolder).FullName.ToLower()))
                        {
                            Console.WriteLine("reset current folder");
                            currentDir = repositoriesFolder;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
          chatId: chatId,
          text: $"{add} not found!",
          cancellationToken: cancellationToken);
                    }
                }
                Message sentMessage = await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "current dir: " + currentDir,
              cancellationToken: cancellationToken);
            }
            else if (messageText.ToLower().StartsWith("cat"))
            {
                var add = messageText.Substring(messageText.IndexOf(' ') + 1).Trim().ToLower();

                var cd = new DirectoryInfo(currentDir);
                if (cd.GetFiles().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
                {
                    var ss = System.IO.File.ReadAllText(Path.Combine(currentDir, add));
                    Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: ss,
                    cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"{add} not found!",
                 cancellationToken: cancellationToken);
                }

            }
            else if (messageText.ToLower().StartsWith("imgcat"))
            {
                var add = messageText.Substring(messageText.IndexOf(' ') + 1).Trim().ToLower();

                var cd = new DirectoryInfo(currentDir);
                if (cd.GetFiles().Any(z => z.Name.ToLower() == add.Trim().ToLower()))
                {
                    var path = Path.Combine(currentDir, add);
                    using (var r = System.IO.File.OpenRead(path))
                    {
                        InputOnlineFile file = new InputOnlineFile(r, Path.GetFileName(path));

                        Message sentMessage = await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: file,
                        cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"{add} not found!",
                 cancellationToken: cancellationToken);
                }

            }
            else
            {

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Sorry, melon. Unknown command\n",
                    cancellationToken: cancellationToken);
            }
            /*
            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText,
                cancellationToken: cancellationToken);*/
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
