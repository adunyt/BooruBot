using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using HentaiBot.Image;

namespace HentaiBot
{
    public class Handlers
    {
        TelegramBotClient botClient;
        NLog.Logger logger;
        public Handlers(Router router, TelegramBotClient currentBotClient, NLog.Logger _logger)
        {
            logger = _logger;
            botClient = currentBotClient;
            router.routes.Add("/start", new Router.Handler(StartHandler));
            router.routes.Add("/help", new Router.Handler(HelpHandler));
            router.routes.Add("/random_image", new Router.Handler(GetImageHandler));
            router.routes.Add("/crash", new Router.Handler(NoCommandError));

        }

        async public Task StartHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Данный бот на начальном этапе разработки, и умеет лишь отправлять рандомные изображения с Danbooru/Gelbooru. Наберите /random_image, чтобы проверить",
                cancellationToken: cancellationToken);
        }

        async public Task GetImageHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Поиск изображения, подождите... ⌛",
                cancellationToken: cancellationToken);

            try
            {
                var result = await ImageFinder.Gelbooru(logger);
                var link = result.Item1;
                var postUri = result.Item2;
                var tags = result.Item3;

                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: link,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    caption: $"<a href=\"{postUri}\">Источник</a>",
                    cancellationToken: cancellationToken);
            }
            finally
            {
                await botClient.DeleteMessageAsync(
                     messageId: sentMessage.MessageId,
                     chatId: chatId,
                     cancellationToken: cancellationToken);
            }
        }

        async public Task HelpHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "/random_image - случайное изображение с Danbooru или Gelbooru\n/help - вывод данного сообщения\n/start - приветственное сообщение",
                cancellationToken: cancellationToken);
        }

        async public Task CrashHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            if (chatId != 349616734)
            {
                return;
            }
            await botClient.SendPhotoAsync(
                chatId: chatId,
                caption: "Force crash",
                photo: "https://i.pinimg.com/originals/96/a4/6c/96a46c2fc626033df7322dce0eae9497.jpg",
                cancellationToken: cancellationToken);
            throw new Exception("Force Crash");
        }

        async public Task NoCommandError(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я не знаю такой команды",
                cancellationToken: cancellationToken);
        }

        //async public Task SendErrorMessageToUserAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        //{
        //    if (update.Message is null)
        //    {
        //        await HandlePollingErrorAsync(botClient, new Exception("Сообщение ничего не содержит, невозможно ответить"), cancellationToken);
        //        return;
        //    }
        //    Message sentMessage = await botClient.SendTextMessageAsync(
        //        chatId: update.Message.Chat.Id,
        //        text: "Произошла попытка при выполнении запроса, повторите попытку ☹️",
        //        replyToMessageId: update.Message.MessageId,
        //        cancellationToken: cancellationToken);
        //}

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            //var ErrorMessage = exception switch
            //{
            //    ApiRequestException apiRequestException
            //        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            //    _ => exception.ToString()
            //};

            //Console.WriteLine($"\nПроизошла критическая ошибка во время работы бота" +
            //    $"\t\nОписание: {ErrorMessage}");
            return Task.CompletedTask;
        }
    }
}
