using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/book")]
    [ApiController]
    public class FeedBacksController : ControllerBase
    {
        private readonly BookDbContext _context;

        public FeedBacksController(BookDbContext context)
        {
            _context = context; 
        }


        [HttpGet("{id}/comment")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer,Administrator")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<FeedBack>>> GetComments(int id,[FromQuery] Paging paging)
        {
            try
            {
                var comments = from c in _context.FeedBacks
                               select c;
                comments = comments.Where(c => c.Book.Id == id)
                        .Include(c => c.User);
                await comments.LoadAsync();
                comments = comments.OrderByDescending(c => c.CreatedDate);
                var data = await PaginatedList<FeedBack>.CreateAsync(comments.AsNoTracking(), paging);
                return Ok(new
                {
                    comments = data,
                    totalPages = data.TotalPages,
                    bookId = id
                });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        [HttpGet("{id}/comment/{commentId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer,Administrator")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> GetComment(int id,int commentId)
        {
            try
            {
                var comment = await _context.FeedBacks.FindAsync(commentId);
                if (comment != null)
                {
                    return Ok(comment);
                }
                return NotFound();    
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("{id}/comment")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer,Administrator")]
        [Produces("application/json")]
        public async Task<IActionResult> CommentBook(int id, [FromQuery]string comment)
        {

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var newFeedBack = new FeedBack()
            {
                Book = book,
                User = user,
                Text = comment

            };
            await _context.FeedBacks.AddAsync(newFeedBack);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (book==null)
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok(book);
        }

        // PUT api/<FeedBacksController>/5
        //[HttpPut("{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer,Administrator")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<FeedBacksController>/5
        [HttpDelete("{id}/comment/{commentId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer,Administrator")]
        public async Task<IActionResult> Delete(int id,int commentId)
        {
            var book = await _context.Books.FindAsync(id);
            var comment = await _context.FeedBacks.FindAsync(commentId);
            if (book==null)
            {
                return NotFound();
            }
            if (comment==null)
            {
                return NotFound();
            }
            try
            {
                
                var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
                if (comment.User.Id!= user.Id)
                {
                    return Unauthorized();
                }
                _context.FeedBacks.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok(book);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
