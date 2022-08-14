using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BooruBot
{
    internal class Listeners
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetLogger("Listeners");
        private Router router;
        public Listeners(Router router)
        {
            this.router = router;
        }
        async public Task MainListener(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            logger.Debug($"Получено обновление, тип - {update.Type}");

            switch (update.Type)
            {
                case UpdateType.Message:
                    var message = update.Message;
                    logger.Debug("Id чата - {chatId}," +
                        " Юзернейм - {username}" +
                        " Имя фамилия пользователя - {firstName} {lastName}",
                        message.Chat.Id, message.Chat.Username ?? "Отстуствует", message.Chat.FirstName ?? "Отстуствует", message.Chat.LastName ?? "Отстуствует");
                    if (message.Text.StartsWith("/") && message.Entities is not null && message.Entities[0].Type is MessageEntityType.BotCommand)
                    {
                        await router.RouteCommand(message, cancellationToken);
                    }
                    else
                    {
                        await router.RouteTextMessage(message, cancellationToken);
                    }
                    break;
                case UpdateType.CallbackQuery:
                    await router.RouteCallback(update.CallbackQuery, cancellationToken, update);
                    break;
                case UpdateType.MyChatMember:
                    await router.RouteMembership(update.MyChatMember, cancellationToken);
                    break;
                default:
                    logger.Error("Не обработанное обновление. Тип {type}", update.Type);
                    break;
            }
        }
    }
}
