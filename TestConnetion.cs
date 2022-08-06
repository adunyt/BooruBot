using BooruSharp.Booru;

namespace HentaiBot
{
    public class TestResult
    {
        public bool Success { get; private set; }
        public Uri Uri { get; private set; }
        public Exception? ConnectException { get; private set; }

        public TestResult(bool success, Uri uri, Exception? exception)
        {
            Success = success;
            Uri = uri;
            ConnectException = exception;
        }

        public override string? ToString()
        {
            var avalible = (Success) ? "доступен" : "недоступен";
            var hasException = (ConnectException is not null) ? $" с ошибкой {ConnectException.Message}" : "";
            return $"{Uri.Host} {avalible}{hasException}";
        }
    }

    public class TestConnetion
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

            var boorus = new List<ABooru>();
            boorus.Add(new DanbooruDonmai());
            boorus.Add(new Gelbooru());
            boorus.Add(new Konachan());

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
