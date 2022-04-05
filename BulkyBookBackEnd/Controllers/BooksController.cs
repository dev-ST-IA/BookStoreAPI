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
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] Paging paging,[FromQuery] string search)
        {
            try
            {
                var books = from b in _context.Books
                            select b;
                if (!String.IsNullOrEmpty(search))
                {
                    books = books.Where(b => b.Title.Contains(search) ||
                                            b.Category.Name.Contains(search) ||
                                            b.Description.Contains(search) ||
                                            b.Price.ToString().Contains(search));
                }
                switch (paging.Sort)
                {
                    case "name_asc":
                        books = books.OrderBy(b => b.Title);
                        break;
                    case "name_desc":
                        books = books.OrderByDescending(b => b.Title);
                        break;
                    case "date_asc":
                        books = books.OrderBy(b => b.CreatedDate);
                        break;
                    case "date_desc":
                        books = books.OrderByDescending(b => b.CreatedDate);
                        break;
                    case "price_asc":
                        books = books.OrderBy(b => b.Price);
                        break;
                    case "price_desc":
                        books = books.OrderByDescending((b) => b.Price);
                        break;
                    default:
                        books = books.OrderByDescending(b => b.Sales);
                        break;
                }
                var data = await PaginatedList<Book>.CreateAsync(books.AsNoTracking(),paging);
                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        // GET: api/Books/5
        [HttpGet("get/{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
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

        [HttpPost("comment/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer,Administrator")]
        public async Task<IActionResult> CommentBook(int id, [FromForm] string comment)
        {

            var book = await GetBook(id);
            if (id != book.Value.Id)
            {
                return BadRequest();
            }

            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var newFeedBack = new FeedBack()
            {
                Book= book.Value,
                User = user,
                Text= comment

            };
            await _context.FeedBacks.AddAsync(newFeedBack);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
