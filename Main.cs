using BooruBot;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#region LoggerInit
var config = new NLog.Config.LoggingConfiguration();
var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"logs/log-{DateTime.Now}.txt" };
var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { Name = "Main"};
config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
LogManager.Configuration = config;
Logger logger = LogManager.GetLogger("Main");
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
 + fix json init
 + catching new users
 + rewrite usergroup and botuser
 + rename all groups to channel
 + сообщение о сбоях (HandlePollingErrorAsync)
 + add constans class
 + tags with /random_image >>> group list
 + loli moment
 + починить экстренную остановку (CancelKeyPress)
 +- fix add in first time to channel
 + redirect arts from /random_image command to UserGroup
 + fix json d bug, when data doesn't write properly >>> fix write to file method, because method override file line by line
 + when setting ends set currentchannel to null
 + add some logger to jsonworker
 + catch block from user
 + fix removing group when kicking by "channel" >>> with linq

 * change list to dictonary
 * support multiply channel
 * choose where to send picture
 * add rating setting
 * add /setting command
 * add /latest
 * add /cancel
 * make args with commands /random /latest
 * send picture with tags 
 * get more than 1 pictures
 * add force setting when user start using bot and ordinary setting when user already using bot
 * rewrite constants
 * support video/gif
 * rewrite funcs with Update? update

 - jsonWorker return List<> >>> rewrited types
 - вынести вспомогательные методы в отдельный файл
*/

#region BotInit
var users = await JsonWorker.GetUsersAsync();
var botClient = new TelegramBotClient(Constants.BOT_TOKEN);
var filter = new ReceiverOptions { AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.MyChatMember, UpdateType.CallbackQuery }/*, ThrowPendingUpdates = true */};
var router = new Router(users);
var handlers = new Handlers(router, botClient, users);
var listeners = new Listeners(router);
logger.Debug("Иницилизация бота - успешно");
#endregion

using var cancelSource = new CancellationTokenSource();

bool shuttingDown = false;

int unknownErrorCount = 0;
while (!shuttingDown) // main cycle
{
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
