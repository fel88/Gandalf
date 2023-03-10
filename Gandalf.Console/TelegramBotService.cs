using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using System.Text;
using LibGit2Sharp;
using System.Xml.Linq;
using Telegram.Bot.Types.InputFiles;
using System.Threading;
using Telegram.Bot.Requests;
using Gandalf.Processors;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Gandalf
{
    public class TelegramBotService : ITelegramBotService
    {
        TelegramBotClient botClient;
        List<ICommandProcessor> Processors = new List<ICommandProcessor>();
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
                    case "gitUsername":
                        gitUsername = vl;
                        break;
                    case "gitBashPath":
                        GitService.GitBashPath = vl;
                        break;
                }
            }
        }

        const string configFileName = "config.xml";
        string apiKey;
        string gitUsername;
        string gitEmail;
        string gitPassword;
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
            CancellationToken = cts.Token;
            Processors.Add(new FindCommandProcessor(this));
            Processors.Add(new FormatCommandProcessor(this));
            Processors.Add(new ImgCatCommandProcessor(this));
            Processors.Add(new ReplaceCommandProcessor(this));
            Processors.Add(new DeleteCommandProcessor(this));
            Processors.Add(new InsertCommandProcessor(this));
            Processors.Add(new CmdCommandProcessor(this));
            Processors.Add(new GhCommandProcessor(this));
            Processors.Add(new GitCommandProcessor(this));
            Processors.Add(new FuncCommandProcessor(this));
            Processors.Add(new PatchCommandProcessor(this));
            Processors.Add(new PingCommandProcessor(this));
            Processors.Add(new CatCommandProcessor(this));
            Processors.Add(new ParseCommandProcessor(this));
            Processors.Add(new EnterFileCommandProcessor(this));
            Processors.Add(new ExitFileCommandProcessor(this));
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            botClient.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions: receiverOptions, cancellationToken: cts.Token);
            var me = await botClient.GetMeAsync();
        }

        string currentDir = "c:\\git";
        public string CurrentDir => currentDir;
        public ITelegramBotClient Bot => botClient;
        public long ChatId => targetChatId;
        public CancellationToken CancellationToken { get; set; }

        public string CurrentFile { get; set; }

        public int CurrentFileLine { get; set; }

        public BotMode Mode { get; set; }

        public bool EchoMode = false;
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

            if (EchoMode)
            {
                Console.WriteLine(messageText);
                return;
            }

            //skip previous messages
            /*if (message.Date < Program.StartTimestamp)
            {
                Console.WriteLine($"skipped: {messageText}");
                return;
            }*/
            var messageTextOrigin = messageText;
            messageText = messageText.ToLower().Trim();
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            bool handled = false;
            foreach (var item in Processors)
            {
                try
                {
                    if (await item.Process(messageTextOrigin))
                    {
                        handled = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ex.Message, cancellationToken: cancellationToken);
                }
            }

            if (handled)
                return;
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

                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: sb.ToString(), cancellationToken: cancellationToken);
            }
            else if (messageText.StartsWith("build"))
            {
                try
                {
                    MsbuildService ms = new MsbuildService();
                    ms.MsBuildPath = msbuildPath;
                    if (messageText.Contains("-verb:errors"))
                    {
                        ms.ErrorsOnly = true;
                    }

                    ms.ErrorsOnly = true;
                    if (messageText.Contains("--help"))
                    {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Build current dir solution with msbuild\nkeys:\n-verb:errors - show only compilation errors", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        GitService gs = new GitService();
                        var res = ms.Build(new RepositoryInfo() { Path = currentDir }, gs).ToString();
                        if (res.Length > Constants.MessageLengthLimit)
                        {
                            for (int i = 0; i < res.Length; i += Constants.MessageLengthLimit)
                            {
                                var res1 = res.Substring(i, Constants.MessageLengthLimit);
                                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: res1, cancellationToken: cancellationToken);
                            }
                        }
                        else
                        {
                            Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: res, cancellationToken: cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: ex.Message, cancellationToken: cancellationToken);
                }
            }
            else if (false && messageText.StartsWith("git checkout"))
            {
                GitService gs = new GitService();
                var rep = new RepositoryInfo()
                {
                    Path = currentDir
                };
                rep.UpdateCommits();
                var cmt = rep.Commits.First();
                gs.Checkout(cmt);
                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "done", cancellationToken: cancellationToken);
            }
            else if (messageText.StartsWith("cd.."))
            {
                var cd = new DirectoryInfo(currentDir);
                currentDir = cd.Parent.FullName;
                if (!new DirectoryInfo(currentDir).FullName.ToLower().StartsWith(new DirectoryInfo(repositoriesFolder).FullName.ToLower()))
                {
                    currentDir = repositoriesFolder;
                }

                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "current dir: " + currentDir, cancellationToken: cancellationToken);
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
                else if (messageText.Contains(' '))
                {
                    var add = messageText.Substring(messageText.IndexOf(' ') + 1);
                    var dd = new DirectoryInfo(currentDir);
                    if (dd.GetDirectories().Any(z => z.Name.ToLower().Contains(add.Trim().ToLower())))
                    {
                        add = dd.GetDirectories().First(z => z.Name.ToLower().Contains(add.Trim().ToLower())).Name;
                        currentDir = Path.Combine(currentDir, add);
                        if (!new DirectoryInfo(currentDir).FullName.ToLower().StartsWith(new DirectoryInfo(repositoriesFolder).FullName.ToLower()))
                        {
                            Console.WriteLine("reset current folder");
                            currentDir = repositoriesFolder;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: $"{add} not found!", cancellationToken: cancellationToken);
                    }
                }

                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "current dir: " + currentDir, cancellationToken: cancellationToken);
            }
            else if (messageText.ToLower().StartsWith("shutdown"))
            {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "terminating...", cancellationToken: cancellationToken);
                Thread th = new Thread(() =>
                {
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                });
                th.IsBackground = true;
                th.Start();
            }
            else
            {
                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "Sorry, melon. Unknown command\n", cancellationToken: cancellationToken);
            }
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()};
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}