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
        private IConfiguration _configuration;

        public CatsController(ILogger<CatsController> logger, IConfiguration configuration, CatsDbContext catsContext, CatsRepository catsRepository)
        {
            _logger = logger;
            _configuration = configuration;
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
        public async Task<IActionResult> GetCats([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 25, [FromQuery] string tag = "")
        {
            if (currentPage < 1)
            {
                return BadRequest("pageNumber cannot be a negative or zero number");
            }

            if (pageSize < 1)
            {
                return BadRequest("currentPage cannot be a negative or zero number");
            }

            try
            {
                var res = await _catsRepository.GetCats(currentPage, pageSize, tag);
                return Ok(res.ToApiResponse());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
