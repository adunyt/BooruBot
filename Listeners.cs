using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HentaiBot
{
    public class Listeners
    {
        Router router;
        NLog.Logger logger;
        public Listeners(Router rt, NLog.Logger _logger)
        {
            logger = _logger;
            router = rt;
        }
        enum MessageTypes
        {
            command,
            text,
            other
        }
        async public Task MainListener(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            logger.Debug($"Получено обновление, тип - {update.Type}");
            MessageTypes messageType;
            if (update.Type is UpdateType.Message && update.Message?.Text is not null)
            {
                var message = update.Message;
                logger.Debug("Id чата - {chatId}," +
                    " Юзернейм - {username}" +
                    " Имя фамилия пользователя - {firstName} {lastName}",
                    message.Chat.Id, message.Chat.Username ?? "Отстуствует", message.Chat.FirstName ?? "Отстуствует", message.Chat.LastName ?? "Отстуствует");
                if (message.Text.StartsWith("/") && message.EntityValues is not null && message.Entities[0].Type is MessageEntityType.BotCommand)
                {
                    messageType = MessageTypes.command;
                    await router.RouteCommand(update, cancellationToken);
                }
                else
                {
                    messageType = MessageTypes.text;
                    //await Router.RouteText(update.Message);
                }
            }
            else
            {
                messageType = MessageTypes.other;
                //await Router.RouteOther(update);
            }
        }
    }
}
