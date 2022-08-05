using Telegram.Bot.Types;

namespace MycollegeBot
{
    public class Router
    {
        public delegate Task Handler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs);

        public Dictionary<string, Handler> routes = new();

        async public Task RouteCommand(Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var command = message.Text.Substring(message.Entities[0].Offset, message.Entities[0].Length);
            Console.WriteLine($"\t Команда: {command}");
            Handler handler = routes.GetValueOrDefault(command);
            await handler(update.Message.Chat.Id, message.MessageId, command, cancellationToken);
        }
    }
}
