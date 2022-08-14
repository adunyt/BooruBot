using BooruSharp.Booru;
using BooruSharp.Search.Post;

namespace BooruBot
{
    static internal class ImageFinder
    {
        async static public Task<(string, string, string[])> Random(ABooru booru, NLog.Logger logger, List<string>? tags = null, Rating rating = Rating.General)
        {
            logger.Info("Поиск случайного изображения на {booru}", booru.BaseUrl.Host);
            tags ??= new();
            SearchResult result;
            switch (rating)
            {
                case Rating.General:
                    tags.Add("rating:general");
                    break;
                case Rating.Safe:
                    tags.Add("rating:safe");
                    break;
                case Rating.Questionable:
                    tags.Add("rating:questionable");
                    break;
                case Rating.Explicit:
                    tags.Add("rating:explicit");
                    break;
            }
            do
            {
                result = await booru.GetRandomPostAsync(tags.ToArray());
            } while (result.PostUrl is null);


            //var result = await booru.GetRandomPostAsync("yuri score:>50 rating:explicit");
            logger.Debug(
                "Post URL: {postUri}\tРейтинг: {rating}\tТеги: {tags}",
                result.PostUrl.AbsoluteUri,
                result.Rating.ToString(),
                string.Join(", ", result.Tags));

            return (result.FileUrl?.AbsoluteUri ?? result.Source, result.PostUrl.AbsoluteUri, result.Tags.ToArray());
        }
    }
}
