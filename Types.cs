using BooruSharp.Booru;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace BooruBot
{
    enum UserAnswer
    {
        Yes,
        No,
        Force
    }

    enum Booru { 
        GelBooru,
        Danbooru
    }

    /// <summary>
    /// Contain message handlers
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="messageId"></param>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="attrs"></param>
    /// <returns></returns>
    internal delegate Task MessageHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs);

    internal delegate Task MembershipHandler(ChatMemberUpdated memberUpdated, CancellationToken cancellationToken, params string[] attrs);

    /// <summary>
    /// Contain result of testing some services
    /// </summary>
    internal class TestResult
    {
        public bool Success { get; private set; }
        public Uri Uri { get; private set; }
        public Exception? ConnectException { get; private set; }

        public TestResult(bool success, Uri uri, Exception? exception)
        {
            Success = success;
            Uri = uri;
            ConnectException = exception;
        }

        public override string? ToString()
        {
            var avalible = (Success) ? "доступен" : "недоступен";
            var hasException = (ConnectException is not null) ? $" с ошибкой {ConnectException.Message}" : "";
            return $"{Uri.Host} {avalible}{hasException}";
        }
    }

    internal class UserGroup
    {
        public UserGroup(long id, long ownerId)
        {
            Id = id;
            OwnerId = ownerId;
        }

        public long Id { get; private set; }
        public long OwnerId { get; set; }
        public HashSet<string> BlacklistTags { get; set; } = new();
        public HashSet<string> PrefersTags { get; set; } = new();
        public List<Booru> Boorus { get; set; } = new();
        public HashSet<BooruSharp.Search.Post.Rating> Ratings { get; set; } = new();
    }

    internal class BotUser
    {
        [JsonConstructor]
        public BotUser(long id)
        {
            Id = id;
        }

        public long Id { get; private set; }

        public bool Block { get; private set; }
        public List<UserGroup> Groups { get; set; } = new();
    }
}
