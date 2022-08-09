using BooruSharp.Booru;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HentaiBot
{
    internal static class JsonWorker
    {

        public static void BackupUsers()
        {
            var jsonFile = File.ReadAllTextAsync("users.json");
            var backupFile = File.WriteAllTextAsync($"users-{DateTime.Now.ToFileTime}.bak", jsonFile.Result);
        }

        async public static Task SaveUserAsync(BotUser botUser)
        {
            var users = GetUsersAsync().Result;
            users.Add(botUser);
            await SaveAllUsersAsync(users);
        }

        async private static Task SaveAllUsersAsync(List<BotUser> users)
        {
            using var jsonFile = File.OpenWrite("users.json");
            await JsonSerializer.SerializeAsync(jsonFile, users);
            await jsonFile.DisposeAsync();
        }

        /// <summary>
        /// Add booru site to UserGroup by userId and groupId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="booru"></param>
        /// <returns>Returns true on success, false on fault</returns>
        async public static Task<bool> AddBooruToUserAsync(long userId, long groupId, Booru booru)
        {
            var users = GetUsersAsync().Result;
            BotUser? user = users.Find(user => user.Id == userId);
            UserGroup? group = user?.Groups.Find(group => group.Id == groupId);
            if (group is not null || !group.Boorus.Contains(booru))
            {
                group.Boorus.Add(booru);
                await SaveAllUsersAsync(users);
                return true;
            }
            else
            {
                return false;
            }
            //users.Remove(user);
            //users.Add(user);
        }

        async public static Task<bool> AddGroupToUserAsync(long userId, long groupId)
        {
            var users = GetUsersAsync().Result;
            BotUser? user = users.Find(user => user.Id == userId);
            if (user is not null || !user.Groups.Exists(group => group.Id == groupId))
            {
                user.Groups.Add(new UserGroup(groupId, userId));
                await SaveUserAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        async public static Task<List<BotUser>> GetUsersAsync()
        {
            if (!File.Exists("users.json") || await File.ReadAllTextAsync("users.json") == "")
            {
                File.WriteAllText("users.json",JsonSerializer.Serialize(new List<BotUser>() { new BotUser(0)}));
            }
            using var jsonFile = File.OpenRead("users.json");
            var users = await JsonSerializer.DeserializeAsync<List<BotUser>>(jsonFile) ?? new();
            await jsonFile.DisposeAsync();
            return users;
        }
    }

}
