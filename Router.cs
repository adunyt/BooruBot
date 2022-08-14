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

        public Dictionary<string, MessageHandler> CommandRoutes { get; private set; } = new();

        public Dictionary<string, MembershipHandler> MemberRoutes { get; private set; } = new();

        public Dictionary<string, SettingsHandler> SettingsRoutes { get; private set; } = new();

        async public Task RouteCommand(Message message, CancellationToken cancellationToken)
        {
            var command = message.Text.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            logger.Debug("Команда: {command}", command);
            var handler = CommandRoutes.GetValueOrDefault(command) ?? CommandRoutes.GetValueOrDefault("unknown"); //если не найден handler для обработки команды, попытка достать специальный для этого handler
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
            if (users.ContainsKey(message.From.Id) && users[message.From.Id].State != BotUserState.Ready)
            {
                var user = users[message.From.Id];
                logger.Info("Получено сообщение для настройки - {message}", message);
                await RouteSetting(message.Text, user, cancellationToken);
            }
        }

        async public Task RouteCallback(CallbackQuery callbackQuery, CancellationToken cancellationToken, Update? update = null) //make overload
        {
            if (users.ContainsKey(callbackQuery.From.Id) && users[callbackQuery.From.Id].State != BotUserState.Ready)
            {
                var user = users[callbackQuery.From.Id];
                await RouteSetting(callbackQuery.Data, user, cancellationToken, update);
            }
        }

        async public Task RouteSetting(string message, BotUser botUser, CancellationToken cancellationToken, Update? update = null) //make overload
        {
            SettingsHandler? settings = botUser.State switch
            {
                BotUserState.SetMode or BotUserState.Start => SettingsRoutes["mode"],
                BotUserState.SetBooru => SettingsRoutes["booru"],
                BotUserState.SetTags => SettingsRoutes["tags"],
                BotUserState.SetBlacklist or BotUserState.SetPreferences => SettingsRoutes["end"],
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
            var handler = MemberRoutes.GetValueOrDefault("me");
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
