using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/sales")]
    [ApiController]
    public class SalesLogsController : ControllerBase
    {
        private readonly BookDbContext _context;

        public SalesLogsController(BookDbContext bookDbContext)
        {
            _context = bookDbContext;
        }
        // GET: api/<SalesLogsController>
        [HttpGet("getAll")]
        [Produces("application/json")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<SalesLog>>> GetSales([FromQuery] Paging paging)
        {
            try
            {
                var sales = from o in _context.SalesLog
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
                //sales = sales.Where(o =>checkRange(o,dateRange));

                sales = sales.Include(e => e.Orders);
                await sales.LoadAsync();
                switch (paging.Sort)
                {
                    case "date_asc":
                        sales = sales.OrderBy(b =>b.Year).ThenBy(x=>x.Month).ThenBy(r=>r.Day);
                        break;
                    case "date_desc":
                        sales = sales.OrderByDescending(b => b.Year).ThenByDescending(x => x.Month).ThenByDescending(r => r.Day);
                        break;
                }
                var data = await PaginatedList<SalesLog>.CreateAsync(sales.AsNoTracking(), paging);
                return Ok(new
                {
                    sales = data,
                    totalPages = data.TotalPages
                });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        // GET api/<SalesLogsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<SalesLogsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SalesLogsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SalesLogsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private bool checkRange(SalesLog o,DateRange dr)
        {
            var date = new DateTime(o.Year, o.Month, o.Day);
            if (date >= dr.Start && date <= dr.End)
            {
                return true;
            }
            return false;
        }
    }
}
