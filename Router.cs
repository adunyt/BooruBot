using Telegram.Bot.Types;

namespace BooruBot
{
    internal class Router
    {
        public Router(Dictionary<long, BotUser> users)
        {
            this.users = users;
        }

        private readonly Dictionary<long, BotUser> users;

        private readonly NLog.Logger logger = NLog.LogManager.GetLogger("Router");

        public Dictionary<string, MessageHandler> commandRoutes = new();

        public Dictionary<string, MembershipHandler> memberRoutes = new();

        public Dictionary<string, SettingsHandler> settingsRoutes = new();

        async public Task RouteCommand(Message message, CancellationToken cancellationToken)
        {
            var command = message.Text.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            logger.Debug("Команда: {command}", command);
            var handler = commandRoutes.GetValueOrDefault(command) ?? commandRoutes.GetValueOrDefault("unknown"); //если не найден handler для обработки команды, попытка достать специальный для этого handler
            try
            {
                await handler(message.Chat.Id, message.MessageId, command, cancellationToken);
            }
            catch (NullReferenceException e)
            {
                logger.Warn(e, "Не опредлен обработчик команды! {command}", command);
            }
        }

        async public Task RouteTextMessage(Message message, CancellationToken cancellationToken)
        {
            if (users.ContainsKey(message.From.Id) && users[message.From.Id].State != BotState.Ready)
            {
                var user = users[message.From.Id];
                logger.Info("Получено сообщение для настройки - {message}", message);
                await RouteSetting(message.Text, user, cancellationToken);
            }
        }

        async public Task RouteCallback(CallbackQuery callbackQuery, CancellationToken cancellationToken, Update? update = null) //make overload
        {
            if (users.ContainsKey(callbackQuery.From.Id) && users[callbackQuery.From.Id].State != BotState.Ready)
            {
                var user = users[callbackQuery.From.Id];
                await RouteSetting(callbackQuery.Data, user, cancellationToken, update);
            }
        }

        async public Task RouteSetting(string message, BotUser botUser, CancellationToken cancellationToken, Update? update = null) //make overload
        {
            SettingsHandler? settings = botUser.State switch
            {
                BotState.SetMode or BotState.Start => settingsRoutes["mode"],
                BotState.SetBooru => settingsRoutes["booru"],
                BotState.SetTags => settingsRoutes["tags"],
                BotState.SetBlacklist or BotState.SetPreferences => settingsRoutes["end"],
                _ => throw new Exception($"Невозможно выполнить направление сообщения настройки, когда пользователь не находится в настройках. Этап - {botUser.State}"),
            };
            try
            {
                await settings(message, botUser, cancellationToken, update);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e, "Не определен handler для настройки {state}", botUser.State);
                throw;
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
