using CatsApi.DataAccess.Entities;

namespace CatsApi.Models
{
    public static class ModelExtensions
    {

        public static CatListResponseModel ToApiResponse(this List<CatEntity> list)
        {
            List<CatResponseModel> catEntities = new List<CatResponseModel>();

            list.ForEach(c =>
            {
                var responseModel = new CatResponseModel()
                {
                    CatId = c.CatId,
                    Width = c.Width.ToString(),
                    Height = c.Height.ToString(),
                    ImageData = c.Image,
                    Created = c.Created
                };
                catEntities.Add(responseModel);
            });

            var response = new CatListResponseModel()
            {
                TotalItems = catEntities.Count,

                Items = catEntities
            };

            return response;
        }
    }
}
