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
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByAdmin([FromQuery] Paging paging, [FromQuery] string search)
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

                if (!String.IsNullOrEmpty(search))
                {
                    orders = orders.Where(o =>o.OrderStatus.Contains(search)||o.TotalPrice.ToString().Contains(search)||o.User.FirstName.Contains(search)||o.User.LastName.Contains(search) );
                }
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
                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        // GET: api/Orders/5
        [HttpGet("get/{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = from o in _context.Orders
                        select o;
            var user = Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var role = user.Result.Role;
            if (role == "Customer")
            {
                order = order.Where(o=>o.Id==id&&o.User.Id==user.Id);
            }else if(role == "Administrator")
            {
                order = order.Where(o => o.Id == id);
            }
            else
            {
                return Unauthorized();
            }
            var data = await order.FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound();
            }

            return data;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("put/{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }
            var user = Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, _context);
            var role = user.Result.Role;
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

            return NoContent();
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
