using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HentaiBot
{
    internal class Listeners
    {
        private readonly NLog.Logger logger;
        private Router router;
        public Listeners(Router router, NLog.Logger logger)
        {
            this.logger = logger;
            this.router = router;
        }
        async public Task MainListener(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            logger.Debug($"Получено обновление, тип - {update.Type}");
            if (update.Type is UpdateType.Message && update.Message?.Text is not null)
            {
                var message = update.Message;
                logger.Debug("Id чата - {chatId}," +
                    " Юзернейм - {username}" +
                    " Имя фамилия пользователя - {firstName} {lastName}",
                    message.Chat.Id, message.Chat.Username ?? "Отстуствует", message.Chat.FirstName ?? "Отстуствует", message.Chat.LastName ?? "Отстуствует");
                if (message.Text.StartsWith("/") && message.Entities is not null && message.Entities[0].Type is MessageEntityType.BotCommand)
                {
                    await router.RouteCommand(message, cancellationToken);
                }
            }
            else if (update.Type == UpdateType.MyChatMember && update.MyChatMember is not null)
            {
                await router.RouteMembership(update.MyChatMember, cancellationToken);
            }
            else
            {
                logger.Error("Не обработанное обновление. Тип {type}", update.Type);
            }
        }
    }
}
