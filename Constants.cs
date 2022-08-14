namespace BooruBot
{
    static public class Constants
    {
        public const long OWNER_ID = 349616734;

        public const string BOT_TOKEN = "5473922129:AAG5oD6OqnVUfR18hNmPMx_U1-WulrYMy-8";
        
        static public Dictionary<MessageToUser, string> MESSAGES = new()
        {
            { MessageToUser.ChannelError, "Произошла ошибка при попытке отправить изображение в канал, удостоверьтесь что вы добавили бота в канал" },
            { MessageToUser.UnknownError, "Произошла непредвиденная ошибка, повторите попытку, или обратитесь к владельцу бота" },
            { MessageToUser.Start, "Привет! Выбери режим использования, чтобы продолжить" },
            { MessageToUser.WaitImage, "Поиск, подождите... ⌛" },
            { MessageToUser.SetBooru, "Выберите, где будут поиск артов" },
        };
    }
}
