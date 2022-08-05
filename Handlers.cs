using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using HentaiBot.Image;

namespace HentaiBot
{
    public class Handlers
    {
        TelegramBotClient botClient;
        public Handlers(Router router, TelegramBotClient currentBotClient)
        {
            botClient = currentBotClient;
            router.routes.Add("/start", new Router.Handler(StartHandler));
            router.routes.Add("/help", new Router.Handler(HelpHandler));
            router.routes.Add("/random_image", new Router.Handler(GetImageHandler));

        }

        async public Task StartHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ну здарова",
                cancellationToken: cancellationToken);
        }

        async public Task GetImageHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Поиск хорошего изображения, секунду... ⌛",
                cancellationToken: cancellationToken);

            var link = await ImageFinder.Gelbooru();


            await botClient.SendPhotoAsync(
                chatId: chatId,
                photo: link,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                caption: $"<a href=\"{link}\">Источник</a>",
                cancellationToken: cancellationToken); 
            await botClient.DeleteMessageAsync(
                 messageId: sentMessage.MessageId,
                 chatId: chatId,
                 cancellationToken: cancellationToken);
        }

        async public Task HelpHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я нахожусь в разработке, но я должен буду уметь:\n* Возможность добавится в группу и отправлять там картинки\n* Иметь интерфейс",
                cancellationToken: cancellationToken);
        }

        async public Task NoCommandError(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я не знаю такой команды",
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
