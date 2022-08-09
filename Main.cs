using HentaiBot;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#region LoggerInit
var config = new NLog.Config.LoggingConfiguration();
var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"logs/log-{DateTime.Now}.txt" };
var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
LogManager.Configuration = config;
Logger logger = LogManager.GetCurrentClassLogger();
logger.Debug("Иницилизация логгера - успешно");
#endregion

/// <summary>
/// Asking user for exit until gets it
/// </summary>
/// <param name="message">Message to user</param>
/// <returns>Return true when user want to exit, false when doesn't</returns>
bool AskYesOrNo(string message)
{
    while (Console.KeyAvailable)
    {
        Console.ReadKey();
    }
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

/// <summary>
/// Asking user for Yes No or Force
/// </summary>
/// <param name="message">Message to user</param>
/// <returns>Return one of UserAnswer</returns>
UserAnswer AskYesNoForce(string message)
{
    while (Console.KeyAvailable)
    {
        Console.ReadKey();
    }
    Console.Write(message);
    string userAnswer = Console.ReadKey().KeyChar.ToString().ToLower();
    Console.WriteLine();
    while (!(userAnswer == "y" || userAnswer == "n" || userAnswer == "f"))
    {
        Console.Write("Введите Y/N/F ");
        userAnswer = Console.ReadKey().KeyChar.ToString().ToLower();
        Console.WriteLine();
    }
    if (userAnswer == "y")
    {
        logger.Debug(@"Пользователь согласился");
        return UserAnswer.Yes;
    }
    else if (userAnswer == "n")
    {
        logger.Debug(@"Пользователь отказал");
        return UserAnswer.No;
    }
    else
    {
        logger.Debug(@"Пользователь принудительно решил запустить");
        return UserAnswer.Force;
    }
}

/// <summary>
/// Upload displayed in chat bot commands to Telegram
/// </summary>
void UpdateCommands(TelegramBotClient bot)
{
    var commands = new List<BotCommand>
    {
        new BotCommand { Command = "/start", Description = "Начать общение с ботом" },
        new BotCommand { Command = "/random_image", Description = "Случайный рисунок" },
        new BotCommand { Command = "/help", Description = "Попросить помощи у бота" }
    };

    bot.SetMyCommandsAsync(commands);
}

/* TODO:
 + вынести все типы в отдельный класс
 + сделать файл с хранением пользователя - групп (Json)
 + сделать файл группа - предпочтения, такие как blacklist, теги, boorus, рейтинг (Json)
 + добавлять, а не перезаписывать json файл
 + add users to handler init
 + определение, что бота добавили в группу (MyChatMember)
 +- redirect arts from /random_image command to UserGroup
 + fix json init
 + catching new users
 * починить экстренную остановку (CancelKeyPress)
 * сообщение о сбоях (HandlePollingErrorAsync)
 * catch block from user
 * tags with /random_image
 * fix add in first time to channel
 * fix not member 
 * rename all groups to channel
 * rewrite usergroup and botuser
 ? jsonWorker return List<>
 - loli moment
 - вынести вспомогательные методы в отдельный файл
*/

#region BotInit
var users = await JsonWorker.GetUsersAsync();
var botClient = new TelegramBotClient("5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8");
var filter = new ReceiverOptions { AllowedUpdates = new UpdateType[2] { UpdateType.Message, UpdateType.MyChatMember }/*, ThrowPendingUpdates = true */};
var router = new Router(logger);
var handlers = new Handlers(router, botClient, users, logger);
var listeners = new Listeners(router, logger);
logger.Debug("Иницилизация бота - успешно");
#endregion

using var cancelSource = new CancellationTokenSource();

bool shuttingDown = false;
bool botRunning = false;
Console.CancelKeyPress += (sender, e) =>
{
    shuttingDown = true;
    if (botRunning)
    {
        logger.Info("Остановка приложения и бота...");
        cancelSource.Cancel();
        e.Cancel = true;
    }
    else
    {
        logger.Info("Остановка приложения...");
        e.Cancel = false;
    }
};

int unknownErrorCount = 0;
while (!shuttingDown) // main cycle
{
    botRunning = false;
    Thread.Sleep(500); // some thread sleeping for except frequently repeated requests on errors
    int apiErrorCount = 0;
    var botCancelToken = cancelSource.Token;
    try
    {
        #region Some tests
        var testConnection = new TestConnetion();
        logger.Debug("Проверка соединения с сайтами");
        if (await testConnection.TestBoorusAsync())
        {
            string servicesWithError = "";
            foreach (var booru in testConnection.Results)
            {
                servicesWithError += booru.ToString() + ", ";
            }
            string message = (testConnection.Results.Count == testConnection.ServicesWithError.Count) ?
                $"Невозможно соеденится со всеми сервисами: {servicesWithError}.\nY/N/F > " :
                $"Невозможно соеденится с одним или несколькими сервисами: {servicesWithError}. \nY/N/F > ";
            UserAnswer userAnswer = AskYesNoForce(message);
            if (userAnswer == UserAnswer.Yes)
            {
                continue;
            }
            else if (userAnswer == UserAnswer.No)
            {
                break;
            }
        }
        logger.Debug("Проверка токена бота");
        if (!await botClient.TestApiAsync())
        {
            throw new ArgumentException("Bot token is invalid");
        }
        botCancelToken.ThrowIfCancellationRequested();
        #endregion

        logger.Info("Запуск бота @{botName}. Чтобы остановить нажмите Ctrl+C", botClient.GetMeAsync().Result.Username);
        botRunning = true;
        await botClient.ReceiveAsync(
            updateHandler: listeners.MainListener,
            errorHandler: handlers.HandlePollingErrorAsync,
            receiverOptions: filter,
            cancellationToken: botCancelToken
        );
    }
    #region Error Catching
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
            if (!AskYesOrNo("Ошибка повторилась более чем 5 раз, приостановка программы до ввода пользователя.\nПерезапустить бота? Y/N "))
            {
                break;
            }
        }
    }
    catch (HttpRequestException e)
    {
        logger.Fatal(e, "Возникла ошибка при отправке запроса на сервер");
        if (!AskYesOrNo($"\nПерезапустить бота? Y/N "))
        {
            break;
        }
    }
    catch (OperationCanceledException e)
    {
        logger.Fatal(e, "Задача отменена до начала ее выполнения");
        if (!AskYesOrNo($"\nПерезапустить бота? Y/N "))
        {
            break;
        }
    }
    catch (Exception e)
    {
        unknownErrorCount++;
        if (unknownErrorCount < 4)
        {
            logger.Error(e, "Возникла непредвиденная ошибка");
        }
        else
        {
            logger.Fatal(e, "Непредвиденная ошибка возникла более трех раз, приостановка выполнения");
            if (!AskYesOrNo($"\nПерезапустить бота? Y/N "))
            {
                break;
            }
        }
    }
    #endregion

    logger.Info("Попытка перезапуска бота...");
}
if (!shuttingDown)
{
    cancelSource.Cancel();
}
LogManager.Shutdown();
logger.Info("Остановка бота по желанию пользователя...");
