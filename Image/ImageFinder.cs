namespace HentaiBot.Image
{
    static public class ImageFinder
    {
        async static public Task<(string, string, string[])> Gelbooru(NLog.Logger logger, string[]? tags = null, BooruSharp.Search.Post.Rating rating = BooruSharp.Search.Post.Rating.Explicit)
        {
            logger.Info("Поиск случайного изображения на Gelbooru");
            var booru = new BooruSharp.Booru.DanbooruDonmai();
            string query = "";
            switch (rating)
            {
                case BooruSharp.Search.Post.Rating.General:
                    query += "rating:general";
                    break;
                case BooruSharp.Search.Post.Rating.Safe:
                    query += "rating:safe";
                    break;
                case BooruSharp.Search.Post.Rating.Questionable:
                    query += "rating:questionable";
                    break;
                case BooruSharp.Search.Post.Rating.Explicit:
                    query += "rating:explicit";
                    break;
            }
            var result = await booru.GetRandomPostAsync(query + String.Join(", ", tags ?? new string[] { "*" }));
            logger.Debug("Post URL: {postUri}\tРейтинг: {rating}\tТеги: {tags}",
                      result.PostUrl?.AbsoluteUri ?? "null", result.Rating.ToString(), String.Join(", ", result.Tags));

            return (result.FileUrl?.AbsoluteUri ?? result.Source, result.PostUrl?.AbsoluteUri ?? result.Source, result.Tags.ToArray());
        }

        async static public Task<(string, string, string[])> Danbooru(NLog.Logger logger, string[]? tags = null, BooruSharp.Search.Post.Rating rating = BooruSharp.Search.Post.Rating.General)
        {
            logger.Info("Поиск случайного изображения на Danbooru");
            return await Gelbooru(logger, tags, rating); // gelbooru and danbooru have same search system
        }
    }
}
