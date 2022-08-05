using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MycollegeBot
{
    public class Listeners
    {
        Router router = new Router();
        enum MessageTypes { 
            command,
            text,
            other
        }
        async public Task MainListener(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Получено обновление, типа - {update.Type}");
            MessageTypes messageType;
            if (update.Type is UpdateType.Message && update.Message?.Text is not null)
            {
                var message = update.Message;
                if (message.Text.StartsWith("/") && message.EntityValues is not null && message.Entities[0].Type is MessageEntityType.BotCommand)
                {
                    messageType = MessageTypes.command;
                    await router.RouteCommand(update);
                }
                else
                {
                    messageType = MessageTypes.text;
                    await Router.RouteText(update.Message);
                }
            }
            else
            {
                messageType = MessageTypes.other;
                await Router.RouteOther(update);
            }
            Console.WriteLine($"\t Тип сообщения: {messageType}");
        }
    }
}
