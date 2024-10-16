using System.Text.Json;
using CatsApi.DataAccess;
using CatsApi.DataAccess.Entities;
using CatsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatsApi.Controllers
{
    [ApiController]
    [Route("/cats")]
    public class CatsController : ControllerBase
    {


        private readonly ILogger<CatsController> _logger;
        private CatsStealer _catsStealer;
        private CatsDbContext _catsContext;

        public CatsController(ILogger<CatsController> logger, CatsDbContext catsContext)
        {
            _logger = logger;
            _catsStealer = new CatsStealer();
            _catsContext = catsContext;
        }

        [HttpGet(Name = "playing arround")]
        public async Task<CatItem[]> Get()
        {

            var res = await _catsStealer.GetCats();

            CatItem[]? catsResponse = JsonSerializer.Deserialize<CatItem[]>(await res.Content.ReadAsStringAsync());
            Array.ForEach<CatItem>(catsResponse, c =>
            {
                List<string> tagList = c.breeds[0].temperament.Split(',').ToList();
                tagList.Add(c.breeds[0].name);

                var tagEntities = new List<TagEntity>();
                for (var i = 0; i < tagList.Count; i++)
                {
                    var entity = new TagEntity()
                    {
                        Name = tagList[i],
                        CreatedAt = DateTime.Now,
                    };

                    tagEntities.Add(entity);
                }

                if (_catsContext.Cat.Where(c => c.CatId == c.CatId).Any())
                {
                    return;
                }

                var cat = new CatEntity
                {
                    CatId = c.id,
                    Width = c.width,
                    Height = c.height,
                    Image = "ok",
                    CreatedAt = DateTime.Now,
                    Tags = tagEntities
                };

                _catsContext.Cat.Add(cat);
                _catsContext.SaveChanges();
            });

            return catsResponse;

        }
    }
}
