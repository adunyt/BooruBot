using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using HentaiBot;

using var cancelSource = new CancellationTokenSource();
var botClient = new TelegramBotClient("5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8");
var filter = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>()};
var router = new Router();
var handlers = new Handlers(router, botClient);
router.noCommandHandler = new Router.Handler(handlers.NoCommandError);
var listeners = new Listeners(router);

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
        Console.WriteLine("Остановка бота...");
        e.Cancel = true;
    };

while (true && !shuttingDown)
{
    Console.WriteLine($"Запуск бота @{botClient.GetMeAsync().Result.Username}\nЧтобы остановить нажмите Ctrl+C");
    Thread.Sleep(500); // some thread sleeping for except frequently repeated requests on errors
    int apiErrorCount = 0;
    try
    {
        await botClient.ReceiveAsync(
            updateHandler: listeners.MainListener,
            errorHandler: handlers.HandlePollingErrorAsync,
            receiverOptions: filter,
            cancellationToken: cancelSource.Token
        );
    }
    catch (ApiRequestException e)
    {
        cancelSource.Cancel();
        if (apiErrorCount < 6)
        {
            apiErrorCount++;
            Console.WriteLine($"Возникла со стороны телеграмма ошибка: {e}");
        }
        else
        {
            if (askForExit("Ошибка повторилась более чем 5 раз, приостановка программы до ввода пользователя.\nПерезапустить бота? Y/N "))
            {
                break;
            }
            else
            {
                continue;
            }
        }
    }
    catch (HttpRequestException e)
    {
        if (askForExit($"Возникла ошибка при отправке запроса на сервер.\n{e}\nПерезапустить бота? Y/N "))
        {
            break;
        }
        else
        {
            continue;
        }
    }
    catch (Exception e)
    {
        cancelSource.Cancel();
        Console.WriteLine($"Возникла ошибка: {e}");
    }
}


/// <summary>
/// Asking user for exit until gets it
/// </summary>
/// <param name="message">Message to user</param>
/// <returns>Return true when user want to exit, false when doesn't</returns>
bool askForExit(string message)
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
        Console.WriteLine("\nПерезагрузка...\n");
        return false;
    }
    else
    {
        return true;
    }
}

Console.WriteLine("Остановка бота по желанию пользователя...");