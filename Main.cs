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
    Thread.Sleep(500); // some thread sleeping for except frequently repeated requests on errors
    int apiErrorCount = 0;
    try
    {
        Console.WriteLine($"Запуск бота @{botClient.GetMeAsync().Result.Username}\nЧтобы остановить нажмите Ctrl+C");
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
            Console.WriteLine("Ошибка повторилась более чем 5 раз, приостановка программы до ввода пользователя. Перезапустить бота? Y/N ");
            var userAnswer = Console.Read().ToString().ToLower();
            while (userAnswer != "y" || userAnswer != "n")
            {
                Console.WriteLine("Введите Y/N ");
                userAnswer = Console.Read().ToString().ToLower();
            }
            if (userAnswer == "y")
            {
                Console.WriteLine("\nПерезагрузка...\n");
                continue;
            }
            else if (userAnswer == "n")
            {
                break;
            }
        }
    }
    catch (Exception e)
    {
        cancelSource.Cancel();
        Console.WriteLine($"Возникла ошибка: {e}");
    }
}

Console.WriteLine("Остановка бота по желанию пользователя...");