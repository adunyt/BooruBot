using BooruSharp.Booru;

namespace HentaiBot
{

    internal class TestConnetion
    {
        public List<TestResult> Results { get; private set; } = new List<TestResult>();
        public List<TestResult> ServicesWithError { get; private set; } = new List<TestResult>();

        /// <summary>
        /// Test Danbooru, Gelbooru, Konachan for connection
        /// </summary>
        /// <returns>Return true if services avalible, false if not avalible</returns>
        async public Task<bool> TestBoorusAsync()
        {
            bool hasException = false;

            var boorus = new List<ABooru>
            {
                new DanbooruDonmai(),
                new Gelbooru(),
                //new Konachan()
            };

            foreach (var booru in boorus)
            {
                bool success;
                Exception? exception = null;
                try
                {
                    await booru.GetRandomPostAsync();
                    success = true;
                }
                catch (Exception e)
                {
                    hasException = true;
                    success = false;
                    exception = e;
                    ServicesWithError.Add(new TestResult(success, booru.BaseUrl, exception));
                }
                var result = new TestResult(success, booru.BaseUrl, exception);
                Results.Add(result);
            }
            return hasException;
        }
    }
}
