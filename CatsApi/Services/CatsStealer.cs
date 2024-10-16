namespace CatsApi.Services
{
    public class CatsStealer
    {

        private static HttpClient client;

        public CatsStealer() {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", "live_apncpQ7YqcTvTzl2FOGG1j56BcYwFxSYA0rPef1HsAL2RtUefNg9OxqCByIoUqYR");
        }


        public async Task<HttpResponseMessage> GetCats()
        {
            return await client.GetAsync("https://api.thecatapi.com/v1/images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit=1");
           
        }
    }


}
