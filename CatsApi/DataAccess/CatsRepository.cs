using System.Text.Json;
using CatsApi.Controllers;
using CatsApi.DataAccess.Entities;
using CatsApi.Services;
using Microsoft.EntityFrameworkCore;

namespace CatsApi.DataAccess
{
    public class CatsRepository
    {

        private readonly ILogger<CatsController> _logger;
        private CatsStealerService _catsStealer;
        private CatsDbContext _catsDbContext;

        public CatsRepository(ILogger<CatsController> logger, CatsDbContext catsDbContext)
        {
            _logger = logger;
            _catsStealer = new CatsStealerService();
            _catsDbContext = catsDbContext;
        }

        public async Task<List<CatEntity>> GetCatById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new List<CatEntity>();
            }

            var cat = _catsDbContext.Cat.AsNoTracking().Where(c => c.CatId == id).FirstOrDefault();

            if (cat != null)
            {
                return await LoadLocalImageData(new List<CatEntity>() { cat });
            }

            return new List<CatEntity>();
        }

        public async Task<List<CatEntity>> GetCats(int page, int pageSize)
        {
            var dbResponse = _catsDbContext.Cat
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return await LoadLocalImageData(dbResponse);
        }

        public async Task<List<CatEntity>> GetCats(int currentPage = 2, int pageSize = 10, string tag = "")
        {
            List<CatEntity> response = new List<CatEntity>();
            if (tag != "")
            {
                var tagOfInterestCats = _catsDbContext.Cat
                    .AsNoTracking()
                    .Where(p => p.Tags.Any(t => t.Name == tag.ToLower()))
                    .OrderBy(c => c.Id)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return await LoadLocalImageData(tagOfInterestCats);
            }

            var allCats = _catsDbContext.Cat
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return await LoadLocalImageData(allCats);
        }

        public async Task<bool> SeedCats(int limit = 25)
        {
            var res = await _catsStealer.GetCats(limit);

            CatItem[]? catsResponse = JsonSerializer.Deserialize<CatItem[]>(await res.Content.ReadAsStringAsync());

            try
            {
                if (catsResponse != null)
                {
                    await SeedDatabase(catsResponse);
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<string> StoreImageLocally(byte[] image, string imageName, string catId)
        {
            var byteStream = new MemoryStream(image);
            string newFileName = catId + "-" + imageName;

            IFormFile file = new FormFile(byteStream, 0, image.Length, "name", newFileName);


            var imageStorageFolderName = "LocalImageRepository";
            Directory.CreateDirectory("LocalImageRepository");

            var filePath = Path.Combine(imageStorageFolderName, newFileName);

            // Create a new file in the home-guac directory with the newly generated file name
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                //copy the contents of the received file to the newly created local file 
                await file.CopyToAsync(stream);
            }
            // return the file name for the locally stored file
            return filePath;
        }

        private bool ImageIsTooBigForTheSameTable(CatItem image)
        {
            const int bytePerPixel = 3;
            const int acceptableKBsToStoreInDb = 50000000;

            var totalPixels = image.width * image.height;
            var totalBytesForJpegFormat = totalPixels * bytePerPixel;

            if (totalBytesForJpegFormat > acceptableKBsToStoreInDb)
            {
                return true;
            }

            return false;
        }

        private async Task StolenCatHandler(CatItem cat)
        {
            {
                // calculate image Size
                // if is greater than ~ 450 Kb then store it directly to DB else store it elsewhere and query it explicitly
                var decision = ImageIsTooBigForTheSameTable(cat);
                if (decision)
                {
                    // Image is too big to store.
                    return;
                }

                string filePath;
                byte[] catImage = [];
                try
                {
                    catImage = await _catsStealer.GetCatImage(cat.url);

                    var imageName = cat.url.Substring(cat.url.LastIndexOf('/') + 1);

                    filePath = await StoreImageLocally(catImage, imageName, cat.id);

                    if (catImage == null)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);

                }

                var existingCat = _catsDbContext.Cat.FirstOrDefault(c => c.CatId == cat.id);

                if (existingCat != null)
                {
                    // cat exists, move to the next one on the loop
                    return;
                }

                List<string> tagNames = cat.breeds[0].temperament.Split(',').Select(s => s.Trim()).ToList();

                tagNames.Add(cat.breeds[0].name);

                var tags = new List<TagEntity>();

                foreach (var tagName in tagNames)
                {
                    var existingTag = _catsDbContext.Tag.FirstOrDefault(t => t.Name == tagName);

                    if (existingTag == null)
                    {
                        var newTag = new TagEntity
                        {
                            Name = tagName.ToLower(),
                            Created = DateTime.Now
                        };
                        tags.Add(newTag);

                        _catsDbContext.Tag.Add(newTag);
                    }
                    else
                    {
                        tags.Add(existingTag);
                    }
                }

                var newCat = new CatEntity
                {
                    CatId = cat.id,
                    Width = cat.width,
                    Height = cat.height,
                    Image = filePath,
                    Created = DateTime.Now,
                    Tags = new List<TagEntity>()
                };

                _catsDbContext.Cat.Add(newCat);

                foreach (var tag in tags)
                {
                    if (!newCat.Tags.Contains(tag))
                    {
                        newCat.Tags.Add(tag);
                    }
                }

                _catsDbContext.SaveChanges();
            }
        }

        private async Task<string> LoadImage(string imageUrl)
        {
            byte[] byt = [];
            try
            {
                byt = await System.IO.File.ReadAllBytesAsync(imageUrl);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
            return Convert.ToBase64String(byt);
        }
        private async Task<List<CatEntity>> LoadLocalImageData(List<CatEntity> list)
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var imageData = await LoadImage(list[i].Image);
                    list[i].Image = imageData;
                }
                return list;
            }
            return new List<CatEntity>();
        }

        private async Task SeedDatabase(CatItem[] stolenCats)
        {
            if (stolenCats?.Length > 0)
            {
                for (var i = 0; i < stolenCats.Length; i++)
                {
                    await StolenCatHandler(stolenCats[i]);
                }
            }

        }
    }
}
