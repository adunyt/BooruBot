using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MycollegeBot
{
    public class Router
    {
        public delegate Task Handler(string command, params string[] attrs);

        public Dictionary<BotCommand, Handler>? routes;

        async public Task RouteCommand(Update update)
        {
            var message = update.Message;
            var command = message.Text.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            Console.WriteLine($"\t Команда: {command}");

        }
    }
}
