using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using HentaiBot;

using var cancelSource = new CancellationTokenSource();
var botClient = new TelegramBotClient("5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8");
var filter = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
var router = new Router();
var handlers = new Handlers(router, botClient);
router.noCommandHandler = new Router.Handler(handlers.NoCommandError);
var listeners = new Listeners(router);

var commands = new List<BotCommand>();
commands.Add(new BotCommand { Command = "/start", Description = "Начать общение с ботом" });
commands.Add(new BotCommand { Command = "/random_image", Description = "Случайный рисунок" });
commands.Add(new BotCommand { Command = "/help", Description = "Попросить помощи у бота" });
botClient.SetMyCommandsAsync(commands);

try
{
    botClient.StartReceiving(
        updateHandler: listeners.MainListener,
        pollingErrorHandler: handlers.HandlePollingErrorAsync,
        receiverOptions: filter,
        cancellationToken: cancelSource.Token
    );
    Console.WriteLine($"Запуск бота {botClient.GetMeAsync().Result.Username}");
}
catch (Exception e)
{
    Console.WriteLine($"❌Произошла критическая ошибка во время работы бота❌" +
        $"\nОписание: {e}");
}
Console.ReadLine();
cancelSource.Cancel();
