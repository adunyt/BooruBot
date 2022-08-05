using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using MycollegeBot;

using var cancelSource = new CancellationTokenSource();
var botClient = new TelegramBotClient("5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8");
var filter = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
var router = new Router();
var handlers = new Handlers(router, botClient);
var listeners = new Listeners(router);

botClient.StartReceiving(
    updateHandler: listeners.MainListener,
    pollingErrorHandler: handlers.HandlePollingErrorAsync,
    receiverOptions: filter,
    cancellationToken: cancelSource.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start horny for @{me.Username}");
Console.ReadLine();
cancelSource.Cancel();
