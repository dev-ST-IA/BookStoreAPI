#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using BulkyBookBackEnd.Req.Bodies;
using BulkyBookBackEnd.Res.Bodies;

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/book")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookDbContext _context;

        public BooksController(BookDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet("getAll")]
        [Produces("application/json")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] Paging paging,[FromQuery] string search="",[FromQuery] int category=-1)
        {
            try
            {
                var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity,_context);
                var books = from b in _context.Books
                        select b;

                // Admin
                var joint = books.Join(_context.Categories, b => b.Category.Id, c => c.Id, (b, c) => new {Book=b,CategoryName=c.Name,CategoryId=c.Id});
                var secondJoin = joint.Join(_context.Author, b => b.Book.Author.Id, c => c.Id, (b, c) => new GetBooksAdmin{ Book = b.Book, CategoryName = b.CategoryName, CategoryId =b.CategoryId, AuthorName=c.Name,AuthorId=c.Id });

                if (!String.IsNullOrEmpty(search))
                {
                    books = books.Where(b => b.Id.ToString().Contains(search)||
                                            b.Title.Contains(search) ||
                                            b.Category.Name.Contains(search) ||
                                            b.Description.Contains(search) ||
                                            b.Price.ToString().Contains(search));

                    secondJoin = secondJoin.Where(b => b.Book.Id.ToString().Contains(search) ||
                                            b.Book.Title.Contains(search) ||
                                            b.Book.Category.Name.Contains(search) ||
                                            b.Book.Description.Contains(search) ||
                                            b.Book.Price.ToString().Contains(search) ||
                                            b.Book.Author.Name.Contains(search));
                }
                if(category != -1)
                {
                    books = books.Where(b=>b.Category.Id==category);
                    secondJoin = secondJoin.Where(b => b.Book.Category.Id == category);
                }
                switch (paging.Sort)
                {
                    case "name_asc":
                        books = books.OrderBy(b => b.Title);
                        secondJoin = secondJoin.OrderBy(b => b.Book.Title);
                        break;
                    case "name_desc":
                        books = books.OrderByDescending(b => b.Title);
                        secondJoin = secondJoin.OrderByDescending(b => b.Book.Title);
                        break;
                    case "date_asc":
                        books = books.OrderBy(b => b.CreatedDate);
                        secondJoin = secondJoin.OrderBy(b => b.Book.CreatedDate);
                        break;
                    case "date_desc":
                        books = books.OrderByDescending(b => b.CreatedDate);
                        secondJoin = secondJoin.OrderByDescending(b => b.Book.CreatedDate);
                        break;
                    case "price_asc":
                        books = books.OrderBy(b => b.Price);
                        secondJoin = secondJoin.OrderBy(b => b.Book.Price);
                        break;
                    case "price_desc":
                        books = books.OrderByDescending((b) => b.Price);
                        secondJoin = secondJoin.OrderByDescending(b => b.Book.CreatedDate);
                        break;
                    default:
                        books = books.OrderByDescending(b => b.Sales);
                        secondJoin = secondJoin.OrderByDescending(b => b.Book.Sales);
                        break;
                }

                if (user?.Role == "Administrator")
                {
                    var data = await PaginatedList<GetBooksAdmin>.CreateAsync(secondJoin.AsNoTracking(), paging);
                    return Ok(new
                    {
                        Books = data,
                        TotalPages = data.TotalPages
                    });

                }
                else
                {
                    var filtered = secondJoin.Select(b => new GetBooksCustomer
                    {
                        Id = b.Book.Id,
                        Title = b.Book.Title,
                        Price = b.Book.Price,
                        CreatedDate = b.Book.CreatedDate,
                        CategoryName = b.CategoryName,
                        Description = b.Book.Description,
                        Units = b.Book.Units,
                        ImageUrl = b.Book.ImageUrl,
                        Publisher = b.Book.Publisher,
                        Rating = b.Book.FinalRating,
                        UpdateDate = b.Book.UpdatedDate,
                        AuthorId= (int)b.AuthorId,
                        AuthorName = b.AuthorName
                    });
                    var data = await PaginatedList<GetBooksCustomer>.CreateAsync(filtered.AsNoTracking(), paging);
                    return Ok(new
                    {
                        Books = data,
                        TotalPages = data.TotalPages,
                    });
                }

                //if (user.Result == null)
                //{
                //    var filtered = data.Select(b => new
                //    {
                //        Id = b.Id,
                //        Title = b.Title,
                //        Price = b.Price,
                //        CreatedDate = b.CreatedDate,   
                //        Category = b.Category,
                //        Description = b.Description,
                //        Units = b.Units,
                //        FeedBacks = b.FeedBacks,
                //        ImageUrl = b.ImageUrl,
                //        Publisher = b.Publisher,
                //        Rating = b.Rating,
                //        UpdatedDate = b.UpdatedDate,
                //    });
                //    return Ok(new
                //    {
                //        Books = filtered,
                //        TotalPages = data.TotalPages,
                //    });
                //}


            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        // GET: api/Books/5
        [HttpGet("get/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        [AllowAnonymous]
        public async Task<ActionResult> GetBook(int id)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var book = from b in _context.Books
                       where b.Id == id
                       select b;
            var joint = book.Join(_context.Categories, b => b.Category.Id, c => c.Id, (b, c) => new { Book = b, CategoryName = c.Name, CategoryId = c.Id });
            var secondJoin = joint.Join(_context.Author, b => b.Book.Author.Id, c => c.Id, (b, c) => new GetBooksAdmin { Book = b.Book, CategoryName = b.CategoryName, CategoryId = b.CategoryId, AuthorName = c.Name, AuthorId = c.Id });

            if (secondJoin == null)
            {
                return NotFound();
            }
            var filteredBook = await secondJoin.FirstOrDefaultAsync();
            if (filteredBook == null)
            {
                return NotFound();
            }
            if (user?.Role!="Administrator")
            { 
                var filtered = new GetBooksCustomer
                {
                    Id = filteredBook.Book.Id,
                    Title = filteredBook.Book.Title,
                    Price = filteredBook.Book.Price,
                    CreatedDate = filteredBook.Book.CreatedDate,
                    CategoryId = filteredBook.CategoryId,
                    CategoryName = filteredBook.CategoryName,
                    Description = filteredBook.Book.Description,
                    Units = filteredBook.Book.Units,
                    ImageUrl = filteredBook.Book.ImageUrl,
                    Publisher = filteredBook.Book.Publisher,
                    Rating = filteredBook.Book.FinalRating,
                    UpdateDate = filteredBook.Book.UpdatedDate,
                    AuthorId= (int)filteredBook.AuthorId,
                    AuthorName = filteredBook?.AuthorName,
                };
                return Ok(filtered);
            }
            return Ok(filteredBook);
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("put")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> PutBook([FromForm]EditBook editables)
        {

            try
            {
                var book = await EditBook.EditAsync(_context, editables);
                if (book != null)
                {
                    return Ok(book);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(editables.Id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        // POST: api/Books
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<Book>> PostBook([FromForm]CreateBook createBook)
        {
            var book = await CreateBook.CreateAsync(_context, createBook);
            if(book != null)
            {
            return CreatedAtAction("GetBook", new { id = book.Id }, book);
            }
            else
            {
                return BadRequest("Book Creation Failed");
            }
        }

        // DELETE: api/Books/5
        [HttpDelete("delete/{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("rate/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> RateBook(int id,[FromQuery] float rate)
        {
            var findBook =  _context.Books.Where(e=>e.Id==id).Include(e=>e.Ratings);
            await findBook.LoadAsync();
            var book  = await findBook.FirstOrDefaultAsync();
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            try
            {
                if (book == null)
                {
                    return NotFound();
                }
                var oldBookRating = await _context.BookRatings.Where(r => r.User == user && r.Book == book).FirstOrDefaultAsync();
                BookRating bookRating;
                if (oldBookRating == null)
                {
                    bookRating = new BookRating()
                    {
                        User = user,
                        UserId = user.Id,
                        Book=book,
                        BookId=book.Id,
                        Rating=rate
                    };
                    await _context.BookRatings.AddAsync(bookRating);
                }
                else
                {
                    bookRating = oldBookRating;
                    bookRating.Rating = rate;
                    _context.Entry(bookRating).State =EntityState.Modified;
                }
                book.Ratings.Add(bookRating);
                _context.Entry(book).State = EntityState.Modified;
                var avgRating = book.Ratings.Average(r=>r.Rating);
                book.FinalRating = avgRating;
                await _context.SaveChangesAsync();
                return Ok(book);

            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("rate/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRating(int id)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound();
                }
                var bookRating = await _context.BookRatings.Where(r => r.User == user && r.Book == book).FirstOrDefaultAsync();
                if(bookRating == null)
                {
                    bookRating = new BookRating()
                    {
                        Rating = 0,
                        Book = book,
                        BookId = book.Id
                    };
                    if(user!= null)
                    {
                        bookRating.User = user;
                        bookRating.UserId = user.Id;
                    }

                }
                return Ok(new
                {
                    rating = bookRating
                });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
