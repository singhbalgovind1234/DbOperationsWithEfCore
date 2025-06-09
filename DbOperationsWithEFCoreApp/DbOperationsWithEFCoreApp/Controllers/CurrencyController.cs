using DbOperationsWithEFCoreApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEFCoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public CurrencyController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            // Constructor logic can be added here if needed
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllCurrencies()
        {
            //var currencies = _appDbContext.Currencies.ToList();
            //var currencies = (from c in _appDbContext.Currencies
            //select c).ToList();

            //var currencies = await _appDbContext.Currencies.ToListAsync();
            //var currencies = await (from c in _appDbContext.Currencies
            //select c).ToListAsync();

            //var currencies = await (from c in _appDbContext.Currencies

            //                        select new Currency()
            //                        {
            //                            Id=c.Id,
            //                            Title= c.Title,
            //                            Description = c.Description

            //                        }
            //                        ).ToListAsync();
            //var currencies = await (from c in _appDbContext.Currencies

            //                        select new 
            //                        {
            //                            c.Id,
            //                            c.Title,
            //                            c.Description

            //                        }
            //                        ).ToListAsync();

            var currencies = await (from c in _appDbContext.Currencies

                                    select new
                                    {
                                        c.Id,
                                        c.Title,
                                        c.Description

                                    }
                                   ).AsNoTracking().ToListAsync();  // it mot mentain the state

            return Ok(currencies);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCurrencyByIdAsync([FromRoute] int id)
        {


            var currencie = await _appDbContext.Currencies.FindAsync(id);


            return Ok(currencie);
        }
        [HttpGet("{name}")]
        public async Task<IActionResult> GetCurrencyByNameAsync([FromRoute] string name)
        {


            //var currencie = await _appDbContext.Currencies.Where(x=>x.Title==name).SingleAsync();
            //var currencie = await _appDbContext.Currencies.Where(x=>x.Title==name).SingleOrDefaultAsync();

            //var currencie = await _appDbContext.Currencies.Where(x=>x.Title==name).FirstOrDefaultAsync();
            var currencie = await _appDbContext.Currencies.FirstOrDefaultAsync(x => x.Title == name);



            return Ok(currencie);
        }
        //[HttpGet("{name}/{Description}")]
        //public async Task<IActionResult> GetCurrencyByNameDescriptionAsync([FromRoute] string name, [FromRoute] string Description)
        //{


        //    //var currencie = await _appDbContext.Currencies.Where(x=>x.Title==name).FirstOrDefaultAsync();
        //    var currencie = await _appDbContext.Currencies.FirstOrDefaultAsync(x => x.Title == name && x.Description== Description);



        //    return Ok(currencie);
        //}

        [HttpGet("{name}/{Description}")]
        public async Task<IActionResult> GetCurrencyByNameDescriptionAsync([FromRoute] string name, [FromQuery] string Description)
        {
            //var currencie = await _appDbContext.Currencies
            //    .FirstOrDefaultAsync(x =>
            //    x.Title == name &&
            //    (string.IsNullOrEmpty(Description) ||
            //    x.Description == Description) // Check if Description is null or empty
            //   );
            var currencie = await _appDbContext.Currencies
               .Where(x =>
               x.Title == name &&
               (string.IsNullOrEmpty(Description) ||
               x.Description == Description) // Check if Description is null or empty
              ).ToListAsync();


            return Ok(currencie);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetCurrencyAll()
        {
            var ids = new List<int> { 1, 2, 3, 4 };
            var result = await _appDbContext.Currencies
               .Where(x =>ids.Contains(x.Id)
              ).ToListAsync();


            return Ok(result);
        }

        [HttpPost("all")]
        public async Task<IActionResult> GetCurrencyAllByIds([FromBody] int[] ids)
        {
            var result = await _appDbContext.Currencies
               .Where(x => ids.Contains(x.Id))
               .Select(x=> new Currency
               {
                   Id=x.Id,
                  Title= x.Title
               }).ToListAsync();


            return Ok(result);
        }
    } 
}
