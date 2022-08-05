using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HentaiBot
{
    public class Listeners
    {
        Router router;
        public Listeners(Router rt)
        {
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
            Console.WriteLine($"Получено обновление, тип - {update.Type}");
            MessageTypes messageType;
            if (update.Type is UpdateType.Message && update.Message?.Text is not null)
            {
                var message = update.Message;
                Console.WriteLine($"\tId чата - {message.Chat.Id}," +
                    $"\n\tЮзернейм - {message.Chat.Username ?? "Отстуствует"}" +
                    $"\n\tИмя фамилия пользователя - {message.Chat.FirstName ?? "Отстуствует"} {message.Chat.LastName ?? "Отстуствует"}");
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
