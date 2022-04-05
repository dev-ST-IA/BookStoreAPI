using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/stats")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly BookDbContext _context;

        public StatsController(BookDbContext bookDbContext)
        {
            this._context = bookDbContext;
        }

        [HttpGet("sales/book/{id}")]
        [Produces("application/json")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> GetSalesByBook(int id, [FromQuery] DateRange dateRange)
        {
            try
            {
                var book = await FindBook(id);
                if (book == null)
                {
                    return NotFound();
                }
                var orders = from order in _context.Orders
                             select order;
                orders = orders.Where(order => order.CartProducts.Any(product => product.Product.Id == id));
                var filteredOrders = orders.Where(order => order.OrderStatus == "Delivered")
                                .Where(order =>
                                    DateOnly.FromDateTime(order.OrderUpdateDate) >= dateRange.Start &&
                                    DateOnly.FromDateTime(order.OrderUpdateDate) <= dateRange.End
                                    );
                var cartProducts = filteredOrders.Select(order => order.CartProducts).AsQueryable();
                var filteredProduct = await cartProducts.Select(p => new
                {
                    book = p.Select(v => v.Product).Where(b => b.Id == id).First(),
                    quantity = p.Select(v => v).Where(b => b.Product.Id == id).First().Quantity,
                    totalPrice = p.Select(v => v).Where(b => b.Product.Id == id).First().TotalPrice
                }).FirstOrDefaultAsync();
                //int totalSales = 0;
                //float totalIncome = 0;
                return Ok(filteredProduct);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("sales")]
        [Produces("application/json")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> GetTotalSales([FromQuery] DateRange dateRange)
        {
            try
            {
                var orders = from order in _context.Orders
                             where order.OrderStatus == "Delivered"
                             select order;

                var count = await orders.CountAsync();

                if (count > 0)
                {
                    var response = await orders
                                .Where(order =>
                                    DateOnly.FromDateTime(order.OrderUpdateDate) >= dateRange.Start &&
                                    DateOnly.FromDateTime(order.OrderUpdateDate) <= dateRange.End
                                    ).Select(order => new { products = order.CartProducts, income = order.CartProducts })
                                    .Select(obj => new { totalSales = obj.products.Sum(i => i.Quantity), income = obj.income }).FirstOrDefaultAsync();
                    return Ok(response);
                }
                else
                {
                    return NoContent();
                }


            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }

        [HttpGet("customers")]
        [Produces("application/json")]
        public async Task<IActionResult> GetTotalCustomers([FromQuery] DateRange dateRange)
        {
            try
            {
                var users = from customer in _context.Users
                            select customer;
                users = users.Where(user=>DateOnly.FromDateTime(user.CreatedDateTime) >=dateRange.Start &&
                                            DateOnly.FromDateTime(user.CreatedDateTime) <=dateRange.End
                                            );
                var count = await users.CountAsync();
                return Ok(count);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        private async Task<Book> FindBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                return book;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
