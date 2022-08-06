using Telegram.Bot.Types;

namespace HentaiBot
{
    public class Router
    {
        public Handler noCommandHandler;

        public NLog.Logger logger;

        public delegate Task Handler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs);

        public Dictionary<string, Handler> routes = new();

        async public Task RouteCommand(Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var command = message.Text.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            logger.Debug("Команда: {command}", command);
            Handler handler = routes.GetValueOrDefault(command);
            if (handler is null && noCommandHandler is not null)
            {
                await noCommandHandler(update.Message.Chat.Id, message.MessageId, command, cancellationToken);
                return;
            }
            else if (handler is null)
            {
                logger.Warn("Не возможно обработать команду {command}", command);
                return;
            }
            await handler(update.Message.Chat.Id, message.MessageId, command, cancellationToken);
        }
    }
}
