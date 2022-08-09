using HentaiBot.Image;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HentaiBot
{
    internal class Handlers
    {
        private readonly TelegramBotClient botClient;
        private readonly NLog.Logger logger;
        private readonly List<BotUser> users;
        public Handlers(Router router, TelegramBotClient botClient, List<BotUser> users, NLog.Logger logger)
        {
            this.users = users;
            this.logger = logger;
            this.botClient = botClient;
            router.commandRoutes.Add("/start", new MessageHandler(StartHandler));
            router.commandRoutes.Add("/help", new MessageHandler(HelpHandler));
            router.commandRoutes.Add("/random_image", new MessageHandler(GetImageHandler));
            router.commandRoutes.Add("/gelbooru", new MessageHandler(BooruHandler));
            router.commandRoutes.Add("/crash", new MessageHandler(NoCommandError));
            router.commandRoutes.Add("/loli", new MessageHandler(AntiLoliHandler));
            router.commandRoutes.Add("unknown", new MessageHandler(NoCommandError));
            router.memberRoutes.Add("me", new MembershipHandler(ChangedMembershipHandler));
        }

        async public Task AntiLoliHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            await using (Stream
                        video = System.IO.File.OpenRead("fakeloli.mp4"),
                        thumb = System.IO.File.OpenRead("thumb.jpg")
                        )
            {
                await botClient.SendVideoAsync(
                    chatId: chatId,
                    video: video,
                    thumb: new InputMedia(thumb, "thumb.jpg"),
                    parseMode: ParseMode.Html,
                    caption: $"Ваш IP адрес был передан в правоохранительные органы, и через несколько минут к вам будет выслана оперативная служба"
                    );
            }
        }

        async public Task BooruHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            await JsonWorker.AddBooruToUserAsync(chatId, -1001629756560, Booru.GelBooru);
        }

        async public Task StartHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            if (!users.Exists(user => user.Id == chatId))
            {
                logger.Info("Добавление нового пользователя с id {id}", chatId);
                var newUser = new BotUser(chatId);
                bool isSaved = await JsonWorker.SaveUserAsync(newUser);
                if (isSaved)
                {
                    users.Add(newUser);
                    await botClient.SendAnimationAsync(
                        chatId: chatId,
                        animation: "https://c.tenor.com/HfOIiR-ig-8AAAAC/tenor.gif",
                        caption: "Привет! Добавь меня в канал, чтобы начать",
                        cancellationToken: cancellationToken
                        );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Произошла ошибка, повторите попытку. Если ошибка повторяется, то о ней уже знают, и в близжайшем времени устранят",
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Наберите /help если нужна подсказка",
                    cancellationToken: cancellationToken);
            }
        }

        async public Task GetImageHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Поиск, подождите... ⌛",
                cancellationToken: cancellationToken);

            try
            {
                var userBot = users.Find(user => user.Id == chatId);
                var userGroups = userBot?.Groups;
                if (userGroups is null)
                {
                    throw new NullReferenceException("У пользователя нет активных групп!");
                }
                foreach (var group in userGroups)
                {
                    var randomBooru = new Random().Next(group.Boorus.Count);
                    BooruSharp.Booru.ABooru booru = new BooruSharp.Booru.DanbooruDonmai();
                    switch (group.Boorus[randomBooru])
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
                    await botClient.SendAnimationAsync(
                        chatId: group.Id,
                        animation: link,
                        parseMode: ParseMode.Html,
                        caption: $"<a href=\"{postUri}\">Источник</a>",
                        cancellationToken: cancellationToken);
                }
            }
            catch (NullReferenceException e)
            {
                logger.Error(e, "У пользователя с id {id} нет активных групп, комманда /random_image не может быть выполнена", chatId);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Произошла ошибка при попытке отправить изображение в группу, удостоверьтесь что вы добавили бота в группу",
                    cancellationToken: cancellationToken
                    );
            }
            catch (Exception e)
            {
                logger.Error(e, "Произошла ошибка при попытке отправить фото по запросу пользователя с id {id}", chatId);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Произошла непредвиденная ошибка, повторите попытку, или обратитесь к владельцу бота",
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

        async public Task HelpHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "/random_image - случайное изображение с Danbooru или Gelbooru\n/help - вывод данного сообщения\n/start - приветственное сообщение",
                cancellationToken: cancellationToken);
        }

        async public Task CrashHandler(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            if (chatId != 349616734)
            {
                return;
            }
            await botClient.SendPhotoAsync(
                chatId: chatId,
                caption: "Force crash",
                photo: "https://i.pinimg.com/originals/96/a4/6c/96a46c2fc626033df7322dce0eae9497.jpg",
                cancellationToken: cancellationToken);
            throw new Exception("Force Crash");
        }

        async public Task NoCommandError(long chatId, int messageId, string command, CancellationToken cancellationToken, params string[] attrs)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я не знаю такой команды",
                cancellationToken: cancellationToken);
        }

        async public Task ChangedMembershipHandler(ChatMemberUpdated membership, CancellationToken cancellationToken, params string[] attrs)
        {
            var whoInvited = membership.From;
            var channel = membership.Chat;
            var newStatus = membership.NewChatMember.Status;
            var oldStatus = membership.OldChatMember.Status;
            var whoInvitedString = $"{whoInvited.FirstName} {whoInvited.LastName ?? "\b"} с id: {whoInvited.Id}";

            logger.Info("Пользователь {user} изменил участие в канале {channel}. Было - {old}, Стало - {new}", whoInvitedString, channel.Title, oldStatus, newStatus);
            var user = users.Find(user => user.Id == whoInvited.Id);

            if (user is not null)
            {
                switch (newStatus)
                {
                    case ChatMemberStatus.Creator:
                    case ChatMemberStatus.Administrator:
                    case ChatMemberStatus.Member:
                        await JsonWorker.AddGroupToUserAsync(user.Id, channel.Id);
                        await botClient.SendAnimationAsync(
                            chatId: whoInvited.Id,
                            animation: "https://i.pinimg.com/originals/ca/91/74/ca9174ba5fb038712fd7fb9b754ce3c9.gif",
                            caption: $"Теперь я могу отправлять арты в канал {channel.FirstName}",
                            cancellationToken: cancellationToken
                            );
                        break;
                    case ChatMemberStatus.Left:
                    case ChatMemberStatus.Kicked:
                        bool isDeleted = await JsonWorker.RemoveGroupFromUserAsync(user.Id, channel.Id);
                        var groups = user.Groups;
                        var group = groups.Find(group => group.Id == channel.Id);
                        if (isDeleted)
                        {
                            groups.Remove(group);
                            await botClient.SendPhotoAsync(
                                chatId: whoInvited.Id,
                                photo: "https://media.tenor.com/images/abe6d1fd116074fa291c05c2dfe9c2c2/tenor.gif",
                                caption: $"Увидимся на другом канале",
                                cancellationToken: cancellationToken
                             );
                        }
                        else
                        {
                            logger.Warn("Произошла ошибка во время удаления группы {name} из списка групп пользователя {name}", channel.FirstName, whoInvitedString);
                            await botClient.SendTextMessageAsync(
                                chatId: whoInvited.Id,
                                text: "Произошла ошибка во время удаления группы из списка. Снова добавьте и удалите бота из группы во избежании проблем",
                                cancellationToken: cancellationToken);
                        }
                        break;
                    case ChatMemberStatus.Restricted:
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
                        logger.Info("Пользователь {user} не является участником бота", whoInvitedString);
                        await botClient.LeaveChatAsync(channel.Id, cancellationToken);
                        logger.Info("Выход из канала {channel} пользователя {user} выполнен", channel.FirstName, whoInvitedString);
                        break;
                }
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
