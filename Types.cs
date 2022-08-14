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

    enum Booru
    {
        GelBooru,
        Danbooru
    }

    enum UsingMode
    {
        Single,
        Channel,
        Both
    }

    enum TagList
    {
        Blacklist,
        Preferences
    }

    enum BotUserState
    {
        Start,
        SetMode,
        WaitForChannel,
        SetBooru,
        SetTags,
        SetBlacklist,
        SetPreferences,
        Ready,
        Settings,
        Block
    }

    public enum BotMessage
    {
        Help,
        ChannelError,
        InitUserError,
        UnknownError,
        Start,
        SetBooru,
        SetBlacklist,
        SetPreferences,
        ChannelAdded,
        ChannelRemoved,
        WaitImage
    }

    enum BotKeyboardText
    {
        SingleMode,
        ChannelMode,
    }

    /// <summary>
    /// Contain message handlers
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="messageId"></param>
    /// <param name="commandAttrs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal delegate Task MessageHandler(long chatId, int messageId, string commandAttrs, CancellationToken cancellationToken);

    internal delegate Task MembershipHandler(ChatMemberUpdated memberUpdated, CancellationToken cancellationToken);

    internal delegate Task SettingsHandler(string message, BotUser botUser, CancellationToken cancellationToken, Update? update);

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

        public override string ToString()
        {
            var avalible = (Success) ? "доступен" : "недоступен";
            var hasException = (ConnectException is not null) ? $" с ошибкой {ConnectException.Message}" : "";
            return $"{Uri.Host} {avalible}{hasException}";
        }
    }

    internal class BotUser
    {
        [JsonConstructor]
        public BotUser(long id)
        {
            Id = id;
        }

        public long Id { get; private set; }

        public bool Block { get; set; }

        public BotUserState State { get; set; } = BotUserState.Start;

        public UsingMode UsingMode { get; set; }

        public long? CurrentChatId { get; set; } = null;

        public Dictionary<long, BotUserChat> Chats { get ; set; } = new(); // TODO: make private, but accessible to json

        public bool AddChat(BotUserChat userChannel)
        {
            if (!Chats.ContainsKey(userChannel.Id))
            {
                Chats.Add(userChannel.Id, userChannel);
                return true;
            }
            else
            {
                return false;
            }

        }

        public void AddBooru(long ChannelId, Booru booru)
        {
            if (Chats.ContainsKey(ChannelId) && !Chats[ChannelId].Boorus.Contains(booru))
            {
                Chats[ChannelId].Boorus.Add(booru);
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public void AddTagList(long ChannelId, TagList list, IEnumerable<string> tags)
        {
            if (Chats.ContainsKey(ChannelId))
            {
                BotUserChat Channel = Chats[ChannelId];
                switch (list)
                {
                    case TagList.Blacklist:
                        Channel.BlacklistTags.UnionWith(tags);
                        break;
                    case TagList.Preferences:
                        Channel.PrefersTags.UnionWith(tags);
                        break;
                }
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public void AddRating(long ChannelId, BooruSharp.Search.Post.Rating rating)
        {
            if (Chats.ContainsKey(ChannelId))
            {
                Chats[ChannelId].Ratings.Add(rating);
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public void RemoveBooru(long ChannelId, Booru booru)
        {
            if (Chats.ContainsKey(ChannelId) && Chats[ChannelId].Boorus.Contains(booru))
            {
                Chats[ChannelId].Boorus.Remove(booru);
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public void RemoveTagList(long ChannelId, TagList list, IEnumerable<string> tags)
        {
            if (Chats.ContainsKey(ChannelId))
            {
                BotUserChat Channel = Chats[ChannelId];
                switch (list)
                {
                    case TagList.Blacklist:
                        Channel.BlacklistTags.ExceptWith(tags);
                        break;
                    case TagList.Preferences:
                        Channel.PrefersTags.ExceptWith(tags);
                        break;
                }
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public List<Booru> GetBooru(long ChannelId)
        {
            if (Chats.ContainsKey(ChannelId))
            {
                return Chats[ChannelId].Boorus;
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public HashSet<string> GetTagList(long ChannelId, TagList list)
        {
            if (Chats.ContainsKey(ChannelId))
            {
                BotUserChat Channel = Chats[ChannelId];
                return list switch
                {
                    TagList.Blacklist => Channel.BlacklistTags,
                    TagList.Preferences => Channel.PrefersTags,
                    _ => throw new Exception($"Такого типа тегов нет в группе с id {ChannelId}!"),
                };
            }
            else
            {
                throw new KeyNotFoundException($"У пользователя нет группы с id {ChannelId}");
            }
        }

        public bool RemoveChat(long ChannelId)
        {
            if (Chats.ContainsKey(ChannelId))
            {
                Chats.Remove(ChannelId);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal class BotUserChat
    {
        [JsonConstructor]
        public BotUserChat(long id, long ownerId)
        {
            Id = id;
            OwnerId = ownerId;
        }

        public long Id { get; private set; }
        public long OwnerId { get; set; }
        public bool IsReady { get; set; } = false;
        public HashSet<string> BlacklistTags { get; set; } = new();
        public HashSet<string> PrefersTags { get; set; } = new();
        public List<Booru> Boorus { get; set; } = new();
        public HashSet<BooruSharp.Search.Post.Rating> Ratings { get; set; } = new();
    }
}
