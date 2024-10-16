namespace CatsApi.Services
{
    public class CatsStealerService
    {

        private static HttpClient client;

        public CatsStealerService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", "live_apncpQ7YqcTvTzl2FOGG1j56BcYwFxSYA0rPef1HsAL2RtUefNg9OxqCByIoUqYR");
        }


        public async Task<HttpResponseMessage> GetCats(int limit)
        {
            try
            {
                var result = await client.GetAsync($"https://api.thecatapi.com/v1/images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit={limit}");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("BadGateway");
            }
        }

        public async Task<byte[]> GetCatImage(string imageUrl)
        {
            try
            {
                var result = await client.GetByteArrayAsync(imageUrl);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("BadGateway");
            }
        }
    }
}
