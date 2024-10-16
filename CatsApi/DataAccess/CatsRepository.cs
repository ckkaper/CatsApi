using System.Text.Json;
using CatsApi.Controllers;
using CatsApi.DataAccess.Entities;
using CatsApi.Services;

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

            var res = _catsDbContext.Cat.Where(c => c.CatId == id).FirstOrDefault();

            if (res != null)
            {
                var imageData = await LoadImage(res.Image);
                res.Image = imageData;
                return new List<CatEntity>() { res };
            }

            return new List<CatEntity>();
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

        public async Task<List<CatEntity>> GetCats(int page = 1, int pageSize = 10)
        {
            var res = _catsDbContext.Cat
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (res != null)
            {
                for (var i = 0; i < res.Count; i++)
                {
                    var imageData = await LoadImage(res[i].Image);
                    res[i].Image = imageData;
                }

                return res;
            }

            return new List<CatEntity>();
        }


        public async Task<List<CatEntity>> GetCats(int page = 2, int pageSize = 10, string tag = "")
        {
            List<CatEntity> response = new List<CatEntity>();
            if (tag != "")
            {
                var tagOfInterest = _catsDbContext.Cat.Where(p => p.Tags.Any(t => t.Name == tag))
                    .OrderBy(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (tagOfInterest != null)
                {
                    for (var i = 0; i < tagOfInterest.Count; i++)
                    {
                        var imageData = await LoadImage(tagOfInterest[i].Image);
                        tagOfInterest[i].Image = imageData;
                    }
                    return tagOfInterest;
                }

                return new List<CatEntity>();
            }

            var res = _catsDbContext.Cat
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (res != null)
            {
                for (var i = 0; i < res.Count; i++)
                {
                    var imageData = await LoadImage(res[i].Image);
                    res[i].Image = imageData;
                }
                return res;
            }

            return new List<CatEntity>();
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
            const int acceptableKBsToStoreInDb = 500000;

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
                            Name = tagName,
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
