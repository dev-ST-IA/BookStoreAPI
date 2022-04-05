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
using System.Security.Claims;

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly BookDbContext _context;

        public CartsController(BookDbContext context)
        {
            _context = context;
        }

        //// GET: api/Carts
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        //{
        //    return await _context.Carts.ToListAsync();
        //}

        // GET: api/Carts/5
        [HttpGet("get")]
        public async Task<ActionResult<Cart>> GetCart(int id)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);

            if (cart == null)
            {
                return NotFound();
            }else if(cart.User.Id != user.Id)
            {
                return BadRequest();
            }

            return cart;
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("add/{product}")]
        public async Task<IActionResult> AddProduct(int product,[FromQuery] int quantity)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);
            if (cart == null)
            {
                cart = new Cart
                {
                    User = user,
                };
                await _context.Carts.AddAsync(cart);
            }
            var realBook = await getProduct(product,quantity);
            if(realBook == null)
            {
                return BadRequest();
            }
            var isProductIn = await _context.Carts.AnyAsync(c => c.Id==cart.Id&&c.Products.Any(item => item.Product == realBook));
            if (isProductIn)
            {
                return await EditProduct(product, quantity);
            }
            var newProduct = new CartProduct(realBook,quantity);
            var newList = cart.Products;
            newList.Add(newProduct);
            cart.Products = newList;
            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (cart==null)
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

        [HttpPut("delete/{product}")]
        public async Task<IActionResult> DeleteProduct(int id, int product)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);
            if (cart == null)
            {
                return BadRequest();
            }
            var realBook = cart.Products.FirstOrDefault(book=>book.Id==product);
            if (realBook == null)
            {
                return BadRequest();
            }
            cart.Products.Remove(realBook);
            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
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

        [HttpPut("edit/{product}")]
        public async Task<IActionResult> EditProduct( int product,[FromBody] int quantity)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);
            if (cart==null)
            {
                return BadRequest();
            }
            var realBook = await getProduct(product, quantity);
            if (realBook == null)
            {
                return BadRequest();
            }
            var isProductIn = cart.Products.Any(c =>c.Product == realBook);
            if (!isProductIn)
            {
                return BadRequest();
            }
            cart.Products.Where(p => p.Id == product).First().Quantity = quantity;
            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (cart==null)
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

        [HttpPut("clear")]
        public async Task<IActionResult> clearCart()
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);
            if (cart == null)
            {
                return BadRequest();
            }
            cart.Products.Clear();
            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (cart == null)
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

        // POST: api/Carts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Cart>> PostCart(Cart cart)
        //{
        //    _context.Carts.Add(cart);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetCart", new { id = cart.Id }, cart);
        //}

        // DELETE: api/Carts/5
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private  bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }

        private async Task<Cart> getCart(User user)
        {
            var cart = await _context.Carts.Where(cart => cart.User == user&& cart.IsOpen).FirstOrDefaultAsync();
            return cart;
        }

        private async Task<Book> getProduct(int id,int quantity)
        {
            return await _context.Books.Where(book=>book.Id==id&&book.Units>=quantity).FirstOrDefaultAsync();
        }

        private bool ProductExists(int id)
        {
            return _context.Books.Any(e=>e.Id == id);
        }
    }
}
