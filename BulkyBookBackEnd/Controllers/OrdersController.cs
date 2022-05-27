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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BookDbContext _context;

        public OrdersController(BookDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet("getAll")]
        [Produces("application/json")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByAdmin([FromQuery] Paging paging, [FromQuery] string search,[FromQuery] DateRange dateRange)
        {
            try
            {
                var orders = from o in _context.Orders
                            select o;
                //var user = Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
                //var role = user.Result.Role;
                //if (role == "Customer")
                //{
                //    orders = orders.Where(o => o.User.Id == user.Id);
                //}else if(role != "Administrator")
                //{
                //    return Unauthorized();  
                //}
                orders = orders.Where(o =>
                            (o.OrderDate >= Convert.ToDateTime(dateRange.Start))&&
                            (o.OrderDate <= Convert.ToDateTime(dateRange.End))
                            );
                if (!String.IsNullOrEmpty(search))
                {
                    orders = orders.Where(o =>o.OrderStatus.Contains(search)||o.TotalPrice.ToString().Contains(search)||o.User.FirstName.Contains(search)||o.User.LastName.Contains(search) );
                }
                orders = orders.Include(e => e.CartProducts)
                            .ThenInclude(p => p.Product)
                            .Include(e=>e.User);
                await orders.LoadAsync();
                switch (paging.Sort)
                {
                    case "name_asc":
                        orders = orders.OrderBy(b => b.User.FirstName);
                        break;
                    case "name_desc":
                        orders = orders.OrderByDescending(b => b.User.FirstName);
                        break;
                    case "date_asc":
                        orders = orders.OrderBy(b => b.OrderDate);
                        break;
                    case "date_desc":
                        orders = orders.OrderByDescending(b => b.OrderDate);
                        break;
                    case "price_asc":
                        orders = orders.OrderBy(b => b.TotalPrice);
                        break;
                    case "price_desc":
                        orders = orders.OrderByDescending((b) => b.TotalPrice);
                        break;
                    default:
                        orders = orders.OrderBy(b => b.OrderDate);
                        break;
                }
                var data = await PaginatedList<Order>.CreateAsync(orders.AsNoTracking(), paging);
                return Ok(new
                {
                    orders=data,
                    totalPages = data.TotalPages
                });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        // GET: api/Orders/5
        [HttpGet("get/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<ActionResult> GetOrder(int id)
        {
            Order order;
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var role = user.Role;
            if (role == "Customer")
            {
                order = _context.Orders.Single(o=>o.Id==id&&o.User.Id==user.Id);
            }else if(role == "Administrator")
            {
                order = _context.Orders.Single(o => o.Id == id);
            }
            else
            {
                return Unauthorized();
            }
            await _context.Entry(order)
                .Collection(w => w.CartProducts)
                .Query()
                .Include(e=>e.Product)
                .LoadAsync();
            
            //order = order
            //    .Include(e => e.CartProducts)
            //    .ThenInclude(p => p.Product);
            //await order.LoadAsync();
            var data = order;
            
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("put/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<IActionResult> PutOrder(int id, [FromQuery] string status="Ordered")
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var order = await _context.Orders.FindAsync(id);
            var role = user.Role;
            if (role == "Customer")
            {
                if(order.User.Id != user.Id)
                {
                    return Unauthorized();
                }
            }else if (role != "Administrator")
            {
                return Unauthorized();
            }
            order.OrderStatus=status;
            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok();
        }

        //// POST: api/Orders
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost("create")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        //public async Task<ActionResult<Order>> PostOrder()
        //{
        //    var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
        //    var cart = await getCart(user);
        //    if(cart == null)
        //    {
        //        return BadRequest("Cart not found");
        //    }
        //    var products = cart.Products;
        //    if(!(products.Count > 0))
        //    {
        //        return BadRequest("Cart is empty");
        //    }
        //    float totalPrice=0;
        //    int totalSales = 0;
        //    foreach (var product in products)
        //    {
        //        totalPrice += product.TotalPrice;
        //        totalSales += product.Quantity;
        //    }
        //    var order = new Order
        //    {
        //        CartProducts = products,
        //        TotalPrice = totalPrice,
        //        TotalSales = totalSales
        //    };
        //    await _context.Orders.AddAsync(order);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        //}

        // DELETE: api/Orders/5
        [HttpDelete("delete/{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("place")]
        //[ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> PlaceOrder()
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var err = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var message = string.Format("User Not Found");
            if (user == null)
            {
                err.Content = new StringContent(message);
                return StatusCode(400, err);
            }
            var cart = await _context.Carts.Where(c => c.User == user && c.IsOpen).FirstOrDefaultAsync();
            if (cart == null)
            {
                message = string.Format("Cannot Find Products, Please Try Again");
                err.Content = new StringContent(message);
                return StatusCode(400, err);
            }
            var cartProducts = await _context.CartProducts.Where(x => x.CartId == cart.Id).ToListAsync();
            if (cartProducts == null || cartProducts.Count <= 0)
            {
                message = string.Format("No Products Found");
                err.Content = new StringContent(message);
                return StatusCode(400, err);
            }
            int totalSales = 0; ;
            float totalPrice = 0;
            foreach (var product in cartProducts)
            {
                await _context.Entry(product).Reference(x => x.Product).LoadAsync();
                var book = product.Product;
                var bookQuantity = product.Quantity;
                var totalPriceOfBook = product.TotalPrice;
                //var findBookEntity = await _context.Books.FindAsync(book.Id);
                book.Units -= bookQuantity;
                book.Sales += bookQuantity;
                totalSales+=bookQuantity;
                totalPrice += totalPriceOfBook;
                _context.Entry(book).State = EntityState.Modified;
            }
            var order = new Order
            {
                User = user,
                CartProducts = cartProducts,
                TotalPrice = totalPrice,
                TotalSales = totalSales
            };
            await _context.Orders.AddAsync(order);
            var day = DateTime.Now.Day;
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            var salesLog = await _context.SalesLog
                .Where(x => x.Day==day&&x.Month==month&&x.Year==year).FirstOrDefaultAsync();
            if (salesLog == null)
            {
                salesLog = new SalesLog
                {
                    Day = day,
                    Month = month,
                    Year = year
                };
                await _context.SalesLog.AddAsync(salesLog);
                await _context.SaveChangesAsync();
            }
            salesLog.Orders.Add(order);
            _context.Entry(salesLog).State = EntityState.Modified;
            cart.IsOpen = false;
            _context.Entry(cart).State =EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        private async Task<Cart> getCart(User user)
        {
            var cart = await _context.Carts.Where(cart => cart.User == user&& cart.IsOpen).FirstOrDefaultAsync();
            return cart;
        }
    }
}
