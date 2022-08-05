using BooruSharp;

namespace HentaiBot.Image
{
    static public class ImageFinder
    {
        async static public Task<string> Gelbooru(BooruSharp.Search.Post.Rating rating = BooruSharp.Search.Post.Rating.General)
        {
            var booru = new BooruSharp.Booru.Gelbooru();
            var result = await booru.GetRandomPostAsync("");

            Console.WriteLine("\t\t\tPost URL: " + result.PostUrl.AbsoluteUri + Environment.NewLine +
                      "\t\t\tImage is safe: " + (result.Rating == BooruSharp.Search.Post.Rating.Safe) + Environment.NewLine +
                      "\t\t\tTags on the image: " + String.Join(", ", result.Tags) + Environment.NewLine);

            return result.FileUrl.AbsoluteUri;
        }
    }
}
