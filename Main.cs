using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MycollegeBot;

using var cancelSource = new CancellationTokenSource();
var botClient = new TelegramBotClient("5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8");
var filter = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
var handlers = new Handlers();
var listeners = new Listeners();

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
