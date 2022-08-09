using Telegram.Bot.Types;

namespace HentaiBot
{
    internal class Router
    {
        public Router(NLog.Logger logger)
        {
            this.logger = logger;
        }

        private readonly NLog.Logger logger;

        public Dictionary<string, MessageHandler> commandRoutes = new();

        public Dictionary<string, UpdateHandler> memberRoutes = new();

        async public Task RouteCommand(Update update, CancellationToken cancellationToken)
        {
            Message message = update.Message;
            var command = message.Text.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            logger.Debug("Команда: {command}", command);
            var handler = commandRoutes.GetValueOrDefault(command) ?? commandRoutes.GetValueOrDefault("unknown"); //если не найден handler для обработки команды, попытка достать специальный для этого handler
            try
            {
                await handler(message.Chat.Id, message.MessageId, command, cancellationToken);
            }
            catch (NullReferenceException)
            {
                logger.Warn("Не опредлен обработчик команды! {command}", command);
            }
        }
        async public Task RouteMembership(Update update, CancellationToken cancellationToken)
        {
            var handler = memberRoutes.GetValueOrDefault("me");
            await handler(update, cancellationToken);
        }

    }
}
