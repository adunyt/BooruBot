using Telegram.Bot.Types;

namespace BooruBot
{
    internal class Router
    {
        public Router(NLog.Logger logger)
        {
            this.logger = logger;
        }

        private readonly NLog.Logger logger;

        public Dictionary<string, MessageHandler> commandRoutes = new();

        public Dictionary<string, MembershipHandler> memberRoutes = new();

        async public Task RouteCommand(Message message, CancellationToken cancellationToken)
        {
            var command = message.Text?.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            logger.Debug("Команда: {command}", command);
            var handler = commandRoutes.GetValueOrDefault(command ?? "unknown"); //если не найден handler для обработки команды, попытка достать специальный для этого handler
            try
            {
                await handler(message.Chat.Id, message.MessageId, command, cancellationToken);
            }
            catch (NullReferenceException e)
            {
                logger.Warn(e, "Не опредлен обработчик команды! {command}", command);
            }
        }

        async public Task RouteMembership(ChatMemberUpdated chatMember, CancellationToken cancellationToken)
        {
            var handler = memberRoutes.GetValueOrDefault("me");
            try
            {
                await handler(chatMember, cancellationToken);
            }
            catch (NullReferenceException e)
            {
                logger.Warn(e, "Не опредлен обработчик добавления в канал! {command}", chatMember);
            }
        }

    }
}
