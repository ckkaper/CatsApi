namespace CatsApi.Models
{
    public class CatListResponseModel
    {
        public int TotalItems { get; set; }
        public List<CatResponseModel> Items { get; set; }
    }
}
