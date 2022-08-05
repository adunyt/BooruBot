using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MycollegeBot
{
    static public class Listeners
    {
        enum MessageTypes { 
            command,
            text,
            other
        }
        async public static Task MainListener(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Получено обновление, типа - {update.Type}");
            MessageTypes messageType;
            if (update.Type is UpdateType.Message && update.Message?.Text is not null)
            {
                var message = update.Message;
                if (message.Text.StartsWith("/"))
                {
                    messageType = MessageTypes.command;
                }
                else
                {
                    messageType = MessageTypes.text;
                }
            }
            else
            {
                messageType = MessageTypes.other;
            }
            Console.WriteLine($"\t Тип сообщения: {messageType}");
        }
    }
}
