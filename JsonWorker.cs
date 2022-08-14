using BooruSharp.Booru;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BooruBot
{
    internal static class JsonWorker
    {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("JsonWorker");

        private static JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        public static void BackupUsers()
        {
            var jsonFile = File.ReadAllTextAsync("users.json");
            File.WriteAllTextAsync($"users-{DateTime.Now.ToFileTime}.bak", jsonFile.Result);
        }

        async public static Task<bool> AddUserAsync(BotUser botUser) 
        {
            var users = GetUsersAsync().Result;
            if (users.ContainsKey(botUser.Id))
            {
                logger.Error("Невозможно добавить пользователя, так как он существует, используйте UpdateUserAsync для обновления пользователя");
                return false;
            }
            users.Add(botUser.Id, botUser);
            bool isOk = await SaveAllUsersAsync(users);
            return isOk;
        }

        async public static Task<bool> UpdateUserAsync(BotUser botUser)
        {
            var users = GetUsersAsync().Result;
            if (!users.ContainsKey(botUser.Id))
            {
                logger.Error("Невозможно обновить пользователя, так как его не существует. Id {id}", botUser.Id);
                return false;
            }
            users[botUser.Id] = botUser;
            bool isOk = await SaveAllUsersAsync(users);
            return isOk;
        }

        async private static Task<bool> SaveAllUsersAsync(Dictionary<long, BotUser> users)
        {
            using var jsonFile = File.Open("users.json", FileMode.Create);
            try
            {
                await JsonSerializer.SerializeAsync(jsonFile, users, options);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, "Не удалось сохранить сериализовать и сохранить файл пользователей");
                return false;
            }
            finally
            {
                await jsonFile.DisposeAsync();
            }
        }

        async public static Task<Dictionary<long, BotUser>> GetUsersAsync()
        {
            if (!File.Exists("users.json") || await File.ReadAllTextAsync("users.json") == "")
            {
                logger.Info("Файл с пользователями отстуствует или пустой, создание нового");
                File.WriteAllText("users.json",JsonSerializer.Serialize(new Dictionary<long, BotUser>()));
            }
            using var jsonFile = File.OpenRead("users.json");
            try
            {
                Dictionary<long, BotUser> users = await JsonSerializer.DeserializeAsync<Dictionary<long, BotUser>>(jsonFile);
                return users;
            }
            catch (NullReferenceException e)
            {
                logger.Fatal(e, "Обработка файла пользователей вернула null!");
                throw;
            }
            finally
            {
                await jsonFile.DisposeAsync();
            }
        }
    }

}
