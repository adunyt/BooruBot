using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using NLog;
using HentaiBot;

#region LoggerInit
var config = new NLog.Config.LoggingConfiguration();
var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"log-{DateTime.Now}.txt" };
var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
LogManager.Configuration = config;
Logger logger = LogManager.GetCurrentClassLogger();
#endregion

using var cancelSource = new CancellationTokenSource();
var botClient = new TelegramBotClient("5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8");
var filter = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>()};
var router = new Router();
var handlers = new Handlers(router, botClient, logger);
router.noCommandHandler = new Router.Handler(handlers.NoCommandError);
router.logger = logger;
var listeners = new Listeners(router, logger);
logger.Debug("Иницилизация бота - успешно");

//var commands = new List<BotCommand>();
//commands.Add(new BotCommand { Command = "/start", Description = "Начать общение с ботом" });
//commands.Add(new BotCommand { Command = "/random_image", Description = "Случайный рисунок" });
//commands.Add(new BotCommand { Command = "/help", Description = "Попросить помощи у бота" });
//botClient.SetMyCommandsAsync(commands);

bool shuttingDown = false;
Console.CancelKeyPress += (sender, e) => 
    {
        shuttingDown = true;
        cancelSource.Cancel();
        logger.Info("Остановка бота...");
        e.Cancel = true;
    };

while (true && !shuttingDown) // main cycle
{
    Thread.Sleep(500); // some thread sleeping for except frequently repeated requests on errors
    int apiErrorCount = 0;
    var botCancelToken = cancelSource.Token;
    try
    {
        botCancelToken.ThrowIfCancellationRequested();
        logger.Info("Запуск бота @{botName}. Чтобы остановить нажмите Ctrl+C", botClient.GetMeAsync().Result.Username, botCancelToken.IsCancellationRequested);
        await botClient.ReceiveAsync(
            updateHandler: listeners.MainListener,
            errorHandler: handlers.HandlePollingErrorAsync,
            receiverOptions: filter,
            cancellationToken: botCancelToken
        );
    }
    catch (ApiRequestException e)
    {
        if (apiErrorCount < 6)
        {
            apiErrorCount++;
            logger.Error(e, "Возникла со стороны телеграмма ошибка: {apiError}");
        }
        else
        {
            logger.Fatal(e, "Ошибка со стороны телеграмма повторилась более чем 5 раз: {apiError}");
            if (askYesOrNo("Ошибка повторилась более чем 5 раз, приостановка программы до ввода пользователя.\nПерезапустить бота? Y/N "))
            {
                continue;
            }
            else
            {
                break;
            }
        }
    }
    catch (HttpRequestException e)
    {
        logger.Fatal(e, "Возникла ошибка при отправке запроса на сервер");
        if (askYesOrNo($"\nПерезапустить бота? Y/N "))
        {
            continue;
        }
        else
        {
            break;
        }
    }
    catch (OperationCanceledException e)
    {
        logger.Fatal(e, "Задача отменена до начала ее выполнения");
        if (askYesOrNo($"\nПерезапустить бота? Y/N "))
        {
            continue;
        }
        else
        {
            break;
        }
    }
    catch (Exception e)
    {
        logger.Error(e, "Возникла непредвиденная ошибка");
    }
    finally
    {
        logger.Info("Попытка перезапуска бота...");
    }
}


/// <summary>
/// Asking user for exit until gets it
/// </summary>
/// <param name="message">Message to user</param>
/// <returns>Return true when user want to exit, false when doesn't</returns>
bool askYesOrNo(string message)
{
    Console.Write(message);
    string userAnswer = Console.ReadKey().KeyChar.ToString().ToLower();
    Console.WriteLine();
    while (!(userAnswer == "y" || userAnswer == "n"))
    {
        Console.Write("Введите Y/N ");
        userAnswer = Console.ReadKey().KeyChar.ToString().ToLower();
        Console.WriteLine();
    }
    if (userAnswer == "y")
    {
        logger.Debug(@"Пользователь согласился. Сообщение - '{message}'", message);
        return true;
    }
    else
    {
        logger.Debug(@"Пользователь отказал. Сообщение - '{message}'", message);
        return false;
    }
}
cancelSource.Cancel();
logger.Info("Остановка бота по желанию пользователя...");