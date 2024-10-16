namespace CatsApi.Models
{
    public class CatResponseModel
    {
        public string CatId { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public string ImageData { get; set; }

        public DateTime Created { get; set; }
    }
}
