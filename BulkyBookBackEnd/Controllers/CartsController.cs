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
    [Route("api/cart")]
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
        [Produces("application / json")]
        public async Task<ActionResult> GetCart()
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cart = await getCart(user);

            return Ok(cart);
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("add/{product}/{quantity}")]
        [Produces("application / json")]
        public async Task<IActionResult> AddProduct(int product, int quantity)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cartProducts = await getCart(user);
            var cart = await _context.Carts.Where(i => i.User == user && i.IsOpen).FirstOrDefaultAsync();
            var realBook = await getProduct(product,quantity);
            if(realBook == null)
            {
                return BadRequest();
            }
            var isProductIn = cartProducts.Any(item => item.Product == realBook);
            if (isProductIn)
            {
                var edit = await EditProduct(product, quantity);
                return edit;
            }
            var newProduct = new CartProduct(realBook,quantity);
            newProduct.CartId = cart.Id;
            await _context.CartProducts.AddAsync(newProduct);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (cartProducts==null)
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
        public async Task<IActionResult> DeleteProduct(int product)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cartProducts = await getCart(user);
            if (cartProducts == null)
            {
                return BadRequest();
            }
            var realBook = cartProducts.FirstOrDefault(book=>book.Product.Id==product);
            if (realBook == null)
            {
                return BadRequest();
            }
            cartProducts.Remove(realBook);
            _context.Entry(realBook).State = EntityState.Deleted;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (cartProducts.Count<=0)
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

        [HttpPut("edit/{product}/{quantity}")]
        [Produces("application / json")]
        public async Task<IActionResult> EditProduct( int product,int quantity)
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var cartProducts = await getCart(user);
            if (cartProducts==null)
            {
                return BadRequest();
            }
            var realBook = await getProduct(product, quantity);
            if (realBook == null)
            {
                return BadRequest();
            }
            var isProductIn = cartProducts.Any(c =>c.Product == realBook);
            if (!isProductIn)
            {
                return BadRequest();
            }
            var editableItem = cartProducts.Where(c => c.Product == realBook).FirstOrDefault();
            editableItem.Quantity = quantity;
            editableItem.TotalPrice = editableItem.Quantity*editableItem.Product.Price;
            _context.Entry(editableItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (cartProducts==null)
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
            var cartProducts = await getCart(user);
            var cart = await _context.Carts.Where(i => i.User == user && i.IsOpen).FirstOrDefaultAsync();
            if (cartProducts == null||cart==null)
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
                if (cartProducts == null||cart==null)
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
        public async Task<IActionResult> DeleteCart()
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            if (user == null)
            {
                return BadRequest();
            }
            var cart = await _context.Carts.Where(x => x.User == user && x.IsOpen).FirstOrDefaultAsync();
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            _context.Entry(cart).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private  bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }

        private async Task<ICollection<CartProduct>> getCart(User user)
        {
            var isUser =await  _context.Users.AnyAsync(u => u.Id == user.Id);
            if (isUser == null)
            {
                return null;
            }
            var cart = await _context.Carts.Where(cart => cart.User == user && cart.IsOpen).FirstOrDefaultAsync();
            if (cart == null)
            {
                var newCart = new Cart { 
                    IsOpen=true,
                    User = user
                };
                await _context.AddAsync(newCart);
                await _context.SaveChangesAsync();
                return newCart.Products;
            }
            var cartProducts = await _context.CartProducts.Where(c => c.CartId == cart.Id).ToListAsync();
            if (cartProducts.Count > 0)
            {
                foreach(var product in cartProducts)
                {
                    await _context.Entry(product).Reference(x=>x.Product).LoadAsync();
                }
            }
            return cartProducts;
            //var oldCart = new Cart
            //{
            //    Id = cart.Id,
            //    IsOpen = cart.IsOpen,
            //    User = user,
            //};
            //if (cartProducts != null)
            //{
            //    oldCart.Products=cartProducts;
            //    _context.Entry(oldCart).State = EntityState.Modified;
            //    await _context.SaveChangesAsync();
            //    return oldCart;
            //}
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
