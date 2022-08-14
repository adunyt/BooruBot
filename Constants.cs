namespace BooruBot
{
    static public class Constants
    {
        public const long OWNER_ID = 349616734;

        public const string BOT_TOKEN = "5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8";
        
        public static Dictionary<BotMessage, string> MESSAGES = new()
        {
            { BotMessage.ChannelError, "Произошла ошибка при попытке отправить изображение в канал, удостоверьтесь что вы добавили бота в канал" },
            { BotMessage.UnknownError, "Произошла непредвиденная ошибка, повторите попытку, или обратитесь к владельцу бота" },
            { BotMessage.Start, "Привет! Выбери режим использования, чтобы продолжить" },
            { BotMessage.WaitImage, "Поиск, подождите... ⌛" },
            { BotMessage.SetBooru, "Выберите, где будут поиск артов" },
        };
    }
}
