using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BooruBot
{
    internal class Handlers
    {
        private readonly TelegramBotClient botClient;
        private readonly NLog.Logger logger = NLog.LogManager.GetLogger("Handlers");
        private readonly Dictionary<long, BotUser> users;
        public Handlers(Router router, TelegramBotClient botClient, Dictionary<long, BotUser> users)
        {
            this.users = users;
            this.botClient = botClient;
            router.commandRoutes.Add("/start", new MessageHandler(Start));
            router.commandRoutes.Add("/help", new MessageHandler(Help));
            router.commandRoutes.Add("/random_image", new MessageHandler(GetImage));
            router.commandRoutes.Add("/loli", new MessageHandler(AntiLoli));
            router.commandRoutes.Add("unknown", new MessageHandler(Unknown));

            router.memberRoutes.Add("me", new MembershipHandler(ChangedMembership));

            router.settingsRoutes.Add("mode", new SettingsHandler(SetMode));
            router.settingsRoutes.Add("booru", new SettingsHandler(SetBooru));
            router.settingsRoutes.Add("tags", new SettingsHandler(SetTags));
            router.settingsRoutes.Add("end", new SettingsHandler(SaveTags));
        }

        #region Setting

        async public Task SetMode(string message, BotUser botUser, CancellationToken cancellationToken, Update? update = null)
        {
            switch (message)
            {
                case "Одиночный 👤":
                    botUser.AddChannel(new UserChannel(botUser.Id, botUser.Id)); //add fake channel, with user id
                    botUser.CurrentGroupId = botUser.Id;
                    botUser.State = BotState.SetBooru;
                    await JsonWorker.UpdateUserAsync(botUser);
                    ReplyKeyboardMarkup booruReply = new(new[] { new KeyboardButton[] { "Gelbooru", "Danbooru" } })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(
                        chatId: botUser.Id,
                        text: "Выбери источник артов. Не знаешь что выбрать? Выбирай Gelbooru.",
                        replyMarkup: booruReply,
                        cancellationToken: cancellationToken
                        );
                    break;
                case "Каналы 👥":
                    botUser.State = BotState.WaitForChannel;
                    await JsonWorker.UpdateUserAsync(botUser);
                    await botClient.SendAnimationAsync(
                        chatId: botUser.Id,
                        animation: "https://cdn.lowgif.com/full/edb46a3f142b62e1-anime-waiting-gif-13-gif-images-download.gif",
                        caption: "Добавь меня в канал, чтобы продолжить или /cancel чтобы вернуться",
                        cancellationToken: cancellationToken
                        );
                    break;
                default:
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Одиночный 👤", "Каналы 👥" } })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(
                        chatId: botUser.Id,
                        text: "Выбери вариант из клавиатуры",
                        replyMarkup: replyKeyboardMarkup,
                        cancellationToken: cancellationToken
                        );
                    break;
            }
        }

        async public Task SetBooru(string message, BotUser botUser, CancellationToken cancellationToken, Update? update = null)
        {
            long currentGroupId = botUser.CurrentGroupId ?? throw new NullReferenceException($"У пользователя с id {botUser.Id} нет активной группы");
            switch (message.ToLower())
            {
                case "gelbooru":
                    botUser.AddBooru(currentGroupId, Booru.GelBooru);
                    await JsonWorker.UpdateUserAsync(botUser);
                    await SendTagsMenu(botUser, cancellationToken);
                    break;
                case "danbooru":
                    botUser.AddBooru(currentGroupId, Booru.GelBooru);
                    botUser.State = BotState.SetTags;
                    await JsonWorker.UpdateUserAsync(botUser);
                    await SendTagsMenu(botUser, cancellationToken);
                    break;
                default:
                    ReplyKeyboardMarkup booruReply = new(new[] { new KeyboardButton[] { "Gelbooru", "Danbooru" } })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(
                        chatId: botUser.Id,
                        text: "Выбери вариант из клавиатуры",
                        replyMarkup: booruReply,
                        cancellationToken: cancellationToken
                        );
                    break;
            }
        }

        async private Task SendTagsMenu(BotUser botUser, CancellationToken cancellationToken)
        {
            botUser.State = BotState.SetTags;
            await JsonWorker.UpdateUserAsync(botUser);
            InlineKeyboardMarkup selectTags = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Blacklist ⛔", callbackData: "blacklist"),
                    InlineKeyboardButton.WithCallbackData(text: "Предпочтения 😏", callbackData: "preferences"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Далее ➡️", callbackData: "skip")
                }
            });
            await botClient.SendTextMessageAsync(
                chatId: botUser.Id,
                text: "Если хочешь изменить теги, то выбери один из вариантов, или нажми далее",
                replyMarkup: selectTags,
                cancellationToken: cancellationToken
                );
        }

        async public Task SaveTags(string message, BotUser botUser, CancellationToken cancellationToken, Update? update = null)
        {
            if (botUser.State == BotState.SetTags && message == "")
            {
                botUser.State = BotState.Ready;
                botUser.CurrentGroupId = null;
                await JsonWorker.UpdateUserAsync(botUser);
                await botClient.SendTextMessageAsync(
                    chatId: botUser.Id,
                    text: $"Готово! Теперь отправь мне /random_image чтобы получить изображение",
                    cancellationToken: cancellationToken,
                    replyMarkup: new ReplyKeyboardRemove()
                    );
            }
            else
            {
                long currentGroupId = botUser.CurrentGroupId ?? throw new NullReferenceException($"У пользователя с id {botUser.Id} нет активной группы");
                var tags = new List<string>(message.Split(", "));
                switch (botUser.State)
                {
                    case BotState.SetBlacklist:
                        botUser.AddTagList(currentGroupId, TagList.Blacklist, tags);
                        break;
                    case BotState.SetPreferences:
                        botUser.AddTagList(currentGroupId, TagList.Preferences, tags);
                        break;
                    default:
                        throw new Exception("У botUser неправильное состояние");
                }
                await JsonWorker.UpdateUserAsync(botUser);
                await botClient.SendTextMessageAsync(
                    chatId: botUser.Id,
                    text: "Готово!",
                    cancellationToken: cancellationToken
                    );
                await SendTagsMenu(botUser, cancellationToken);
            }
        }

        async public Task SetTags(string message, BotUser botUser, CancellationToken cancellationToken, Update? update = null)
        {
            if (update?.CallbackQuery?.Message is null)
            {
                logger.Error("В {method} передан пустой (null) Update или CallbackQuery или Message", nameof(SetTags));
                throw new ArgumentNullException(paramName: nameof(update), message: "Update.CallbackQuery должен быть не null для данной функции");
            }
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken);
            string text;
            switch (message)
            {
                case "blacklist":
                    botUser.State = BotState.SetBlacklist;
                    text = "не хочешь видеть";
                    break;
                case "preferences":
                    botUser.State = BotState.SetPreferences;
                    text = "ты хотел бы видеть";
                    break;
                case "skip":
                    await SaveTags("", botUser, cancellationToken);
                    return;
                default:
                    logger.Warn("В аргументе message должна быть одна из следующий строчек: blacklist, preferences, skip");
                    return;
            }
            await JsonWorker.UpdateUserAsync(botUser);
            await botClient.EditMessageTextAsync(
                chatId: botUser.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                text: $"Введи теги, которые {text}, через запятую.\nНапример: <code>2girls, nude</code>",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
                );
        }
        #endregion

        #region Commands

        async public Task Start(long chatId, int messageId, string commandAttrs, CancellationToken cancellationToken)
        {
            if (!users.ContainsKey(chatId))
            {
                logger.Info("Добавление нового пользователя с id {id}", chatId);
                var newUser = new BotUser(chatId);
                bool isSaved = await JsonWorker.AddUserAsync(newUser);
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton[] {"Одиночный 👤", "Каналы 👥"}
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                };
                if (isSaved)
                {
                    await botClient.SendAnimationAsync(
                        chatId: chatId,
                        animation: "https://c.tenor.com/HfOIiR-ig-8AAAAC/tenor.gif",
                        caption: Constants.MESSAGES[MessageToUser.Start],
                        replyMarkup: replyKeyboardMarkup,
                        cancellationToken: cancellationToken
                        );
                    newUser.State = BotState.SetMode;
                    users.Add(chatId, newUser);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: Constants.MESSAGES[MessageToUser.UnknownError],
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                if (users.ContainsKey(chatId) && users[chatId].State == BotState.Ready)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Набери /settings если хочешь изменить настройки",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Сначала заверши настройку",
                        cancellationToken: cancellationToken);
                }
            }
        }

        async public Task Help(long chatId, int messageId, string commandAttrs, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "/random_image - случайное изображение с Danbooru или Gelbooru\n/help - вывод данного сообщения\n/start - приветственное сообщение",
                cancellationToken: cancellationToken);
        }

        async public Task GetImage(long chatId, int messageId, string commandAttrs, CancellationToken cancellationToken)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: Constants.MESSAGES[MessageToUser.WaitImage],
                cancellationToken: cancellationToken);

            try
            {
                if (!users.ContainsKey(chatId))
                {
                    logger.Error("Нет зарегистрированного пользователя с id {id}", chatId);
                    return;
                }
                var userBot = users[chatId];
                if (userBot.State != BotState.Ready)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы должны завершить настройку, перед тем как искать картинки!",
                        cancellationToken: cancellationToken);
                    return;
                }
                var userChannels = userBot.Channels;
                foreach (var Channel in userChannels.Values) // TODO: make choose to group
                {
                    var randomBooru = new Random().Next(Channel.Boorus.Count);
                    BooruSharp.Booru.ABooru booru = new BooruSharp.Booru.DanbooruDonmai();
                    switch (Channel.Boorus[randomBooru])
                    {
                        case Booru.GelBooru:
                            booru = new BooruSharp.Booru.Gelbooru();
                            break;
                        case Booru.Danbooru:
                            booru = new BooruSharp.Booru.DanbooruDonmai();
                            break;
                    }
                    var result = await ImageFinder.Abooru(booru, logger);
                    var link = result.Item1;
                    var postUri = result.Item2;
                    var tags = result.Item3;
                    await botClient.SendPhotoAsync(
                        chatId: Channel.Id,
                        photo: link,
                        parseMode: ParseMode.Html,
                        caption: $"<a href=\"{postUri}\">Источник</a>",
                        cancellationToken: cancellationToken);
                }
            }
            catch (NullReferenceException e)
            {
                logger.Error(e, "У пользователя с id {id} нет активных каналов, комманда /random_image не может быть выполнена", chatId);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: Constants.MESSAGES[MessageToUser.ChannelError],
                    cancellationToken: cancellationToken
                    );
            }
            catch (Exception e)
            {
                logger.Error(e, "Произошла ошибка при попытке отправить фото по запросу пользователя с id {id}", chatId);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: Constants.MESSAGES[MessageToUser.UnknownError],
                    cancellationToken: cancellationToken
                    );
            }
            finally
            {
                await botClient.DeleteMessageAsync(
                     messageId: sentMessage.MessageId,
                     chatId: chatId,
                     cancellationToken: cancellationToken);
            }
        }

        async public Task Unknown(long chatId, int messageId, string commandAttrs, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я не знаю такой команды",
                cancellationToken: cancellationToken);
        }

        async public Task AntiLoli(long chatId, int messageId, string commandAttrs, CancellationToken cancellationToken)
        {
            await using Stream
                        video = System.IO.File.OpenRead("fakeloli.mp4"),
                        thumb = System.IO.File.OpenRead("thumb.jpg");
            await botClient.SendVideoAsync(
                chatId: chatId,
                video: video, // TODO: figure it out
                thumb: new InputMedia(thumb, "thumb.jpg"),
                parseMode: ParseMode.Html,
                caption: $"Ваш IP адрес был передан в правоохранительные органы, и через несколько минут к вам будет выслана оперативная служба",
                cancellationToken: cancellationToken
                );
        }
        #endregion

        async public Task ChangedMembership(ChatMemberUpdated membership, CancellationToken cancellationToken)
        {
            User initiator = membership.From;
            Chat chat = membership.Chat;
            ChatMemberStatus newStatus = membership.NewChatMember.Status;
            ChatMemberStatus oldStatus = membership.OldChatMember.Status;
            string initiatorStr = $"{initiator.FirstName} {initiator.LastName ?? "\b"} с id: {initiator.Id}";

            logger.Info("Пользователь {user} изменил участие в канале {channel}. Было - {old}, Стало - {new}", initiatorStr, chat.Title, oldStatus, newStatus);

            if (users.ContainsKey(initiator.Id))
            {
                BotUser user = users[initiator.Id];
                switch (newStatus)
                {
                    case ChatMemberStatus.Creator:
                    case ChatMemberStatus.Administrator:
                    case ChatMemberStatus.Member:
                        if (initiator.Id == chat.Id) // fix of bug that occurs when user choose a channel mode and restart bot
                        {
                            return;
                        }
                        user.AddChannel(new UserChannel(chat.Id, user.Id));
                        user.CurrentGroupId = chat.Id;
                        user.State = BotState.SetBooru;
                        await JsonWorker.UpdateUserAsync(user);
                        ReplyKeyboardMarkup booruReply = new(new[] { new KeyboardButton[] { "Gelbooru", "Danbooru" } })
                        {
                            ResizeKeyboard = true
                        };
                        await botClient.SendAnimationAsync(
                            chatId: initiator.Id,
                            animation: "https://i.pinimg.com/originals/ca/91/74/ca9174ba5fb038712fd7fb9b754ce3c9.gif",
                            caption: $"Теперь я могу отправлять арты в канал {chat.Title}. Выбери источник артов. Не знаешь что выбрать? Выбирай Gelbooru",
                            replyMarkup: booruReply,
                            cancellationToken: cancellationToken
                            );
                        break;
                    case ChatMemberStatus.Left:
                    case ChatMemberStatus.Kicked:
                        if (initiator.Id == chat.Id) // user blocks bot
                        {
                            logger.Info("Пользователь {user} заблокировал бота", initiatorStr);
                            user.Block = true;
                            await JsonWorker.UpdateUserAsync(user);
                            return;
                        }
                        user.RemoveChannelIfExist(chat.Id);
                        bool isDeleted = await JsonWorker.UpdateUserAsync(user);
                        if (isDeleted)
                        {
                            await botClient.SendAnimationAsync(
                                chatId: initiator.Id,
                                animation: "https://media.tenor.com/images/abe6d1fd116074fa291c05c2dfe9c2c2/tenor.gif",
                                caption: $"Увидимся на другом канале",
                                cancellationToken: cancellationToken
                             );
                        }
                        else
                        {
                            logger.Warn("Произошла ошибка во время удаления группы {name} из списка групп пользователя {name}", chat.FirstName, initiatorStr);
                            await botClient.SendTextMessageAsync(
                                chatId: initiator.Id,
                                text: "Произошла ошибка во время удаления группы из списка. Снова добавьте и удалите бота из группы во избежании проблем",
                                cancellationToken: cancellationToken);
                        }
                        break;
                    case ChatMemberStatus.Restricted:
                        // check for ability send text message
                        break;
                }
            }
            else
            {
                switch (newStatus)
                {
                    case ChatMemberStatus.Creator:
                    case ChatMemberStatus.Administrator:
                    case ChatMemberStatus.Member:
                        if (initiator.Id != chat.Id)
                        {
                            logger.Info("Пользователь {user} не является участником бота", initiatorStr);
                            await botClient.LeaveChatAsync(chat.Id, cancellationToken);
                            logger.Info("Выход из канала {channel} пользователя {user} выполнен", chat.FirstName, initiatorStr);
                        }
                        break;
                    case ChatMemberStatus.Left:
                    case ChatMemberStatus.Kicked:
                        logger.Info("Поиск группы и удаление у пользователя, id которого телеграм не дал");
                        foreach (BotUser usr in users.Values)
                        {
                            usr.RemoveChannelIfExist(chat.Id);
                        }
                        break;
                }
            }
        }

        async public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: Constants.OWNER_ID,
                text: $"Произошла ошибка из-за которой бот упал - {exception}",
                cancellationToken: cancellationToken
                );
        }
    }
}
