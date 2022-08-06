using BooruSharp;

namespace HentaiBot.Image
{
    static public class ImageFinder
    {
        async static public Task<(string, string, string[])> Gelbooru(string[]? tags = null, BooruSharp.Search.Post.Rating rating = BooruSharp.Search.Post.Rating.Explicit)
        {
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
            var result = await booru.GetRandomPostAsync(query + String.Join(", ", tags ?? new string[] {"*"}));
            Console.WriteLine("\t\t\tPost URL: " + result.PostUrl?.AbsoluteUri ?? "null" + Environment.NewLine +
                      "\t\t\tРейтинг: " + result.Rating.ToString() + Environment.NewLine +
                      "\t\t\tТеги: " + String.Join(", ", result.Tags) + Environment.NewLine);

            return (result.FileUrl?.AbsoluteUri ?? result.Source, result.PostUrl?.AbsoluteUri ?? result.Source, result.Tags.ToArray());
        }

        async static public Task<(string, string, string[])> Danbooru(string[]? tags = null, BooruSharp.Search.Post.Rating rating = BooruSharp.Search.Post.Rating.General)
        {
            return await Gelbooru(tags, rating); // gelbooru and danbooru have same search system
        }
    }
}
