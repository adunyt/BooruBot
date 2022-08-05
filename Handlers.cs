using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace MycollegeBot
{
    public class Handlers
    {
        TelegramBotClient botClient;
        public Handlers(Router router, TelegramBotClient currentBotClient)
        {
            botClient = currentBotClient;
            router.routes.Add("/start", new Router.Handler(StartHandler));
        }

        async public Task StartHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ну здарова",
                cancellationToken: cancellationToken);
        }

        async public Task SendErrorMessageToUserAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is null)
            {
                await HandlePollingErrorAsync(botClient, new Exception("Сообщение ничего не содержит, невозможно ответить"), cancellationToken);
                return;
            }
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Произошла попытка при выполнении запроса, повторите попытку ☹️",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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
