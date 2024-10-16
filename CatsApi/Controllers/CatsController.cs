using CatsApi.DataAccess;
using CatsApi.Models;
using CatsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatsApi.Controllers
{
    [ApiController]
    [Route("/api")]
    public class CatsController : ControllerBase
    {


        private readonly ILogger<CatsController> _logger;
        private CatsStealerService _catsStealer;
        private CatsDbContext _catsDbContext;
        private CatsRepository _catsRepository;

        public CatsController(ILogger<CatsController> logger, CatsDbContext catsContext, CatsRepository catsRepository)
        {
            _logger = logger;
            _catsStealer = new CatsStealerService();
            _catsDbContext = catsContext;
            _catsRepository = catsRepository;
        }

        [HttpGet("/cats/fetch")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var res = await _catsRepository.SeedCats(25);
                return Ok(true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        [HttpGet("/cats/{id}")]
        public async Task<IActionResult> GetCatById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            try
            {
                var res = await _catsRepository.GetCatById(id);
                return Ok(res.ToApiResponse());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("/cats")]
        public async Task<IActionResult> GetCats([FromQuery] int page = 25, [FromQuery] int pageSize = 1, [FromQuery] string tag = "")
        {
            try
            {
                var res = await _catsRepository.GetCats(page, pageSize, tag);
                return Ok(res.ToApiResponse());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
