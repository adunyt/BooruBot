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

        async public static Task<bool> SaveUserAsync(BotUser botUser) 
        {
            var users = GetUsersAsync().Result;
            if (!users.Exists(usr => usr.Id == botUser.Id))
            {
                users.Add(botUser);
                bool isOk = await SaveAllUsersAsync(users);
                return isOk;
            }
            else
            {
                return false;
            }
        }

        async private static Task<bool> SaveAllUsersAsync(List<BotUser> users)
        {
            using var jsonFile = File.OpenWrite("users.json");
            try
            {
                await JsonSerializer.SerializeAsync(jsonFile, users);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                await jsonFile.DisposeAsync();
            }
        }

        /// <summary>
        /// Add booru site to UserGroup by userId and groupId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="booru"></param>
        /// <returns>Returns true on success, false on fault</returns>
        async public static Task<bool> AddBooruToUserAsync(long userId, long groupId, Booru booru) // !!! TODO: test !!!
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

        async public static Task<bool> RemoveGroupFromUserAsync(long userId, long groupId) // !!! TODO: test !!!
        {
            var users = GetUsersAsync().Result;
            BotUser? user = users.Find(user => user.Id == userId);
            var groups = user?.Groups;
            var group = groups.Find(group => group.Id == groupId);
            if (group is not null)
            {
                groups.Remove(group);
                await SaveUserAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        async public static Task<bool> AddGroupToUserAsync(long userId, long groupId) // !!! TODO: test !!!
        {
            var users = GetUsersAsync().Result;
            BotUser? user = users.Find(user => user.Id == userId);
            var groups = user?.Groups;
            if (groups?.Exists(group => group.Id == groupId) is not null && groups.Count < 5)
            {
                groups.Add(new UserGroup(groupId, userId));
                await SaveUserAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        async public static Task<List<BotUser>> GetUsersAsync() // !!! TODO: test !!!
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
