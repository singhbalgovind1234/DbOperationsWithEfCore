using DbOperationsWithEFCoreApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEFCoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(AppDbContext appDbContext) : ControllerBase
    {
        [HttpGet("")]
        public async Task<ActionResult> GetAllBookList()
        {
            var res = appDbContext.Database.SqlQuery<Book>($"Select * from Books").ToListAsync();
            var res1 = appDbContext.Database.SqlQuery<int>($"Select Id from Books").ToListAsync();

            var res2 = appDbContext.Books.FromSql($"Select * from Books").ToListAsync();

            var res3 = appDbContext.Database.ExecuteSqlAsync($"update NoOfPages=100 from Books where Id=7");


            var res4 = appDbContext.Books.FromSql($"EXEC SP_Books 1").LoadAsync();


            var Id = new SqlParameter("@Id", 1);
            var Title = new SqlParameter("@Title", 1);
            var res5 = appDbContext.Books.FromSqlRaw($"EXEC SP_Books @Id,@Title", Id,Title).LoadAsync();


            //var result = appDbContext.Books.  //use anominous method
            //    Select( x=>new
            //    {
            //        x.Id,
            //        x.Title,
            //        x.Description

            //    }).ToListAsync();
            //var result = appDbContext.Books.  //with type safe 
            //Select(x => new Book()
            //{
            //   Id= x.Id,
            //   Title= x.Title,
            //   Description = x.Description

            //});

            var result = appDbContext.Books.  //with related table data
           Select(x => new
           {
               Id = x.Id,
               Title = x.Title,
               Description = x.Description,
               Author = x.Author != null ? x.Author.Name : "Na",    //navgation
               // Language = x.Language                       

           }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("lazyLoading")]
        public async Task<ActionResult> GetAllBookListlazyLoading()
        {
            // Reference, collection, load one to one,
            var result = await appDbContext.Books.FindAsync(7);

            var author = result.Author;

            return Ok(result);
        }
        [HttpGet("ExlictLoading")]
        public async Task<ActionResult> GetAllBookListExlictLoading()
        {
            // Reference, collection, load one to one,
            var result = await appDbContext.Books.FirstAsync();

            await appDbContext.Entry(result).Reference(x => x.Author).LoadAsync();
            //await appDbContext.Entry(result).Reference(x => x.Language).LoadAsync();

            return Ok(result);
        }



        [HttpGet("ExlictLoadingLanguage")]
        public async Task<ActionResult> GetAllBookListExlictLoadinglanguage()
        {

            // one to many, many to many    

            var languages = await appDbContext.Languages.ToListAsync();
            foreach (var language in languages)
            {
                //await appDbContext.Entry(language).Collection(x=>x.Books)
                //    .Query()
                //    .Where(x=>x.Id==1)
                //    .LoadAsync();
            }

            return Ok(languages);
        }



        [HttpGet("Eager")]
        public async Task<ActionResult> GetAllBookListusingEargerLoading()
        {
            var result = await appDbContext.Books.Where(x=>x.Id==7)
              .Include(x => x.Author)
              .FirstAsync();
            // Use include for eager loading basicaly in inclued related table if used proper foregin key concept

            // we use theninclude for depth means Author also coantain foregin relation to another table

            // Language has on to many relation there it give the error so, it is very careful to use include in linq query
            //var result = await appDbContext.Books
            //  .Include(x => x.Author)
            //  .Include(x => x.Language)                      // Use include for eager loading basicaly in inclued related table if used proper foregin key concept
            //  .ToListAsync();


            

            return Ok(result);
        }



        //work for one to many
        [HttpGet("languages")]
        public async Task<ActionResult> GetAlllanguagesEargerLoading()
        {
            var result = await appDbContext.Languages
              //.Include(x => x.Books)
              .ToListAsync();

            return Ok(result);
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNewBooksAsync([FromBody] Book book)
        {
            await appDbContext.Books.AddAsync(book);
            await appDbContext.SaveChangesAsync();
            return Ok(book);
        }


        [HttpPost("bulk")]
        public async Task<IActionResult> AddNewBooksAsync([FromBody] List<Book> book)
        {
            await appDbContext.Books.AddRangeAsync(book);
            await appDbContext.SaveChangesAsync();
            return Ok(book);
        }
        [HttpPost("addWithAuthor")]
        public async Task<IActionResult> AddBooksAsync([FromBody] Book book)
        {
            var author = new Author
            {
                Name = "Author Name",
                Email = "wewe@author.com"
            };
            book.Author = author;
            await appDbContext.Books.AddRangeAsync(book);
            await appDbContext.SaveChangesAsync();
            return Ok(book);
        }

        [HttpPut("{bookId}")]
        public async Task<IActionResult> UpdateBookAsync([FromRoute] int bookId, [FromBody] Book book)
        {
            var existingBook = await appDbContext.Books.FirstOrDefaultAsync(x => x.Id == bookId);
            if (existingBook == null)
            {
                return NotFound($"Book with ID {bookId} not found.");
            }
            existingBook.Title = book.Title;
            existingBook.Description = book.Description;

            await appDbContext.SaveChangesAsync();
            return Ok(book);
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateBookWithSingleQueryAsync([FromBody] Book book)
        {
            //appDbContext.Books.Update(book);
            appDbContext.Entry(book).State = EntityState.Modified; ;


            await appDbContext.SaveChangesAsync();
            return Ok(book);
        }

        [HttpPut("bulkUpdate")]
        public async Task<IActionResult> BulkUpdateBookWithSingleQueryAsync()
        {
            //await appDbContext.Books.ExecuteUpdateAsync(x => x
            //.SetProperty(p => p.Title, p=>p.Title + "A"));

            await appDbContext.Books
                .Where(x => x.NoOfPages == 100)
                .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.Description, p => p.Title + "New Description for all 7"));

            return Ok();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDeleteAsync()
        {


            var book = await appDbContext.Books.FindAsync(1); //remove book with ID 1
            if (book != null)
            {
                appDbContext.Books.Remove(book);
                await appDbContext.SaveChangesAsync();
            }

            appDbContext.Entry(book).State = EntityState.Deleted;
            await appDbContext.SaveChangesAsync();

            var books = await appDbContext.Books.Where(x=>x.Id<5).ToListAsync(); //remove bulk books with ID less than 5
            if (books.Any())
            {
                appDbContext.Books.RemoveRange(books);
                await appDbContext.SaveChangesAsync();
            }

            appDbContext.Books.Where(x => x.Id < 8).ExecuteDeleteAsync(); // in a single query delet all record

            return Ok();
        }
    }
}
