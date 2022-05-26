using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BulkyBookBackEnd.Req.Bodies;
using BulkyBookBackEnd.Res.Bodies;

namespace BulkyBookBackEnd.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BookDbContext db;
        public UsersController(BookDbContext bookDbContext)
        {
            this.db = bookDbContext;
        }
        // Get All Users
        [HttpGet("customer/getAll")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> GetAllCustomers([FromQuery] Paging paging, [FromQuery] string? search,[FromQuery]DateRange dateRange)
        {
            var users = from u in db.Users
                        where u.Role=="Customer"
                        select u;
            users = users.Where(o =>
                            (o.CreatedDateTime >= dateRange.Start) &&
                            (o.CreatedDateTime <= dateRange.End)
                            );
            try
            {
                if (!String.IsNullOrEmpty(search))
                {
                    users = users.Where(u=>u.UserName.Contains(search)||u.FirstName.Contains(search)||u.LastName.Contains(search)||u.EmailAddress.Contains(search));
                }
                switch (paging.Sort)
                {
                    case "name_asc":
                        users = users.OrderBy(b => b.FirstName);
                        break;
                    case "name_desc":
                        users = users.OrderByDescending(b => b.FirstName);
                        break;
                    case "date_asc":
                        users = users.OrderBy(b => b.CreatedDateTime);
                        break;
                    case "date_desc":
                        users = users.OrderByDescending(b => b.CreatedDateTime);
                        break;
                    default:
                        users = users.OrderBy(b => b.FirstName);
                        break;
                }
                var data = await PaginatedList<User>.CreateAsync(users.AsNoTracking(), paging);
                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            return Ok(users);
        }

        // Get All Users
        [HttpGet("admin/getAll")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> GetAllAdmins([FromQuery] Paging paging, [FromQuery] string? search)
        {
            var users = from u in db.Users
                        where u.Role=="Administrator"
                        select u;
            try
            {
                if (!String.IsNullOrEmpty(search))
                {
                    users = users.Where(u => u.UserName.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search) || u.EmailAddress.Contains(search));
                }
                switch (paging.Sort)
                {
                    case "name_asc":
                        users = users.OrderBy(b => b.FirstName);
                        break;
                    case "name_desc":
                        users = users.OrderByDescending(b => b.FirstName);
                        break;
                    case "date_asc":
                        users = users.OrderBy(b => b.CreatedDateTime);
                        break;
                    case "date_desc":
                        users = users.OrderByDescending(b => b.CreatedDateTime);
                        break;
                    default:
                        users = users.OrderBy(b => b.FirstName);
                        break;
                }
                var data = await PaginatedList<User>.CreateAsync(users.AsNoTracking(), paging);
                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            return Ok(users);
        }

        // Get User
        [HttpGet("get")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<IActionResult> GetUser()
        {
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, db);
            return Ok(user);
        }

        [HttpPost]
        [Route("create/customer")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateUser(CreateUser createUser)
        {
            try
            {
                if (createUser.Role != "Customer")
                {
                    return BadRequest("Cannot Register User");
                }
                var isExist = createUser.checkIfExist(db);
                if (isExist)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new { title = "User Already Exists",status= StatusCodes.Status401Unauthorized });
                }
                byte[] salt = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }
                var userSalt = Convert.ToBase64String(salt);
                //user.Salt = userSalt;
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: createUser.Password,
                                salt: salt,
                                prf: KeyDerivationPrf.HMACSHA256,
                                iterationCount: 100000,
                                numBytesRequested: 256 / 8));
                //user.Password = hashed;
                var newUser = new User
                {
                    FirstName=createUser.FirstName,
                    LastName=createUser.LastName,
                    EmailAddress=createUser.EmailAddress,
                    PhoneNumber=createUser.PhoneNumber,
                    Role=createUser.Role,
                    UserName=createUser.UserName

                };
                await db.Users.AddAsync(newUser);
                var credentials = new Credentials
                {
                    User = newUser,
                    Password = hashed,
                    Salt = userSalt
                };
                await db.Credentials.AddAsync(credentials);
                await db.SaveChangesAsync();

                var claims = Jwt.generateClaims(newUser);
                var token = Jwt.generateToken(claims);
                var tokenString = Jwt.generateTokenString(token);

                var obj = new Dictionary<string, string>();
                obj.Add("token", tokenString);
                obj.Add("emailAddress", newUser.EmailAddress);
                obj.Add("role", newUser?.Role);
                obj.Add("phoneNumber", newUser.PhoneNumber.ToString());
                obj.Add("firstName", newUser.FirstName);
                obj.Add("lastName", newUser.LastName);
                obj.Add("userName", newUser.UserName);
                obj.Add("Id", newUser.Id.ToString());
                return Ok(obj);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("create/admin")]
        [Produces("application/json")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<IActionResult> CreateAdmin(CreateUser createUser)
        {
            try
            {
                createUser.Role = "Administrator";
                if (createUser.Role != "Administrator")
                {
                    return BadRequest("Cannot Register User");
                }
                var isExist = createUser.checkIfExist(db);
                if (isExist)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new { title = "User Already Exists", status = StatusCodes.Status401Unauthorized });
                }
                byte[] salt = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }
                var userSalt = Convert.ToBase64String(salt);
                //user.Salt = userSalt;
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: createUser.Password,
                                salt: salt,
                                prf: KeyDerivationPrf.HMACSHA256,
                                iterationCount: 100000,
                                numBytesRequested: 256 / 8));
                //user.Password = hashed;
                var newUser = new User
                {
                    FirstName = createUser.FirstName,
                    LastName = createUser.LastName,
                    EmailAddress = createUser.EmailAddress,
                    PhoneNumber = createUser.PhoneNumber,
                    Role = createUser.Role,
                    UserName = createUser.UserName

                };
                await db.Users.AddAsync(newUser);
                var credentials = new Credentials
                {
                    User = newUser,
                    Password = hashed,
                    Salt = userSalt
                };
                await db.Credentials.AddAsync(credentials);
                await db.SaveChangesAsync();

                var claims = Jwt.generateClaims(newUser);
                var token = Jwt.generateToken(claims);
                var tokenString = Jwt.generateTokenString(token);

                var obj = new Dictionary<string, string>();
                obj.Add("token", tokenString);
                obj.Add("emailAddress", newUser.EmailAddress);
                obj.Add("role", newUser?.Role);
                obj.Add("phoneNumber", newUser.PhoneNumber.ToString());
                obj.Add("firstName", newUser.FirstName);
                obj.Add("lastName", newUser.LastName);
                obj.Add("userName", newUser.UserName);
                obj.Add("Id", newUser.Id.ToString());
                return Ok(obj);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("login/customer")]
        [Produces("application/json")]

        public async Task<IActionResult> LoginCustomer(UserLogin login)
        {
            try
            {
                var existingUserAcc = await db.Users.FirstOrDefaultAsync(user => (user.EmailAddress == login.EmailAddress && user.Role == "Customer")|| (user.UserName == login.UserName && user.Role == "Customer"));
                if (existingUserAcc == null)
                {
                    return Unauthorized();
                }
                var existingUser = await db.Credentials.FirstOrDefaultAsync(user => user.User.EmailAddress == login.EmailAddress || user.User.UserName == login.UserName);
                if (existingUser == null)
                {
                    return Unauthorized();
                }
                var existingSalt = existingUser?.Salt;
                if (existingSalt == null)
                {
                    return Unauthorized();
                }
                var parsedSalt = Convert.FromBase64String(existingSalt);
                var incomingPassHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                        password: login.Password,
                                        salt: parsedSalt,
                                        prf: KeyDerivationPrf.HMACSHA256,
                                        iterationCount: 100000,
                                        numBytesRequested: 256 / 8));
                if (existingUser.Password.Equals(incomingPassHashed))
                {
                    var oldUser = await db.Users.FirstOrDefaultAsync(u => login.EmailAddress == u.EmailAddress || login.UserName == u.UserName);

                    var claims = Jwt.generateClaims(existingUser.User);
                    var token = Jwt.generateToken(claims);
                    var tokenString = Jwt.generateTokenString(token);
                    var obj = new Dictionary<string, string>();
                    obj.Add("token", tokenString);
                    obj.Add("emailAddress", existingUser.User.EmailAddress);
                    obj.Add("role", existingUser.User.Role);
                    obj.Add("phoneNumber", existingUser.User.PhoneNumber.ToString());
                    obj.Add("firstName", existingUser.User.FirstName);
                    obj.Add("lastName", existingUser.User.LastName);
                    obj.Add("userName", existingUser.User.UserName);
                    obj.Add("Id", existingUser.Id.ToString());
                    return Ok(obj);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("login/admin")]
        [Produces("application/json")]

        public async Task<IActionResult> LoginAdmin(UserLogin login)
        {
            try
            {
                var existingUserAcc = await db.Users.FirstOrDefaultAsync(user => (user.EmailAddress == login.EmailAddress && user.Role == "Administrator") || (user.UserName == login.UserName && user.Role == "Administrator"));
                if (existingUserAcc == null)
                {
                    return Unauthorized();
                }
                var existingUser = await db.Credentials.FirstOrDefaultAsync(user => user.User.EmailAddress == login.EmailAddress || user.User.UserName == login.UserName);
                if (existingUser == null)
                {
                    return Unauthorized();
                }
                var existingSalt = existingUser?.Salt;
                if (existingSalt == null)
                {
                    return Unauthorized();
                }
                var parsedSalt = Convert.FromBase64String(existingSalt);
                var incomingPassHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                        password: login.Password,
                                        salt: parsedSalt,
                                        prf: KeyDerivationPrf.HMACSHA256,
                                        iterationCount: 100000,
                                        numBytesRequested: 256 / 8));
                if (existingUser.Password.Equals(incomingPassHashed))
                {
                    var oldUser = await db.Users.FirstOrDefaultAsync(u => login.EmailAddress == u.EmailAddress || login.UserName == u.UserName);

                    var claims = Jwt.generateClaims(existingUser.User);
                    var token = Jwt.generateToken(claims);
                    var tokenString = Jwt.generateTokenString(token);
                    var obj = new Dictionary<string, string>();
                    obj.Add("token", tokenString);
                    obj.Add("emailAddress", existingUser.User.EmailAddress);
                    obj.Add("role", existingUser.User.Role);
                    obj.Add("phoneNumber", existingUser.User.PhoneNumber.ToString());
                    obj.Add("firstName", existingUser.User.FirstName);
                    obj.Add("lastName", existingUser.User.LastName);
                    obj.Add("userName", existingUser.User.UserName);
                    obj.Add("Id", existingUser.Id.ToString());
                    return Ok(obj);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        // DELETE: api/Orders/5
        [HttpDelete("delete/{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("star/{bookId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> StarBook(int bookId)
        {
            var book = await db.Books.FindAsync(bookId);
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, db);
            if (book == null)
            {
                return BadRequest();
            }
            try
            {
                var previousFound = user.WatchList?.FirstOrDefault(x => x == book);
                if (previousFound == null)
                {
                    if (user.WatchList == null)
                    {
                        user.WatchList = new List<Book>();
                    }
                   user.WatchList.Add(book);  
                }
                else
                {
                    user.WatchList.Remove(previousFound);
                }
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Ok(book);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("star/{bookId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> IsStarBook(int bookId)
        {
            var book = await db.Books.FindAsync(bookId);
            var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, db);
            if (book == null)
            {
                return BadRequest();
            }
            try
            {
                var previousFound = user.WatchList?.FirstOrDefault(x => x == book);
                if (previousFound == null)
                {
                    return Ok(new
                    {
                        isStar=false,
                        book=book
                    });
                }
                else
                {
                    return Ok(new
                    {
                        isStar = true,
                        book = book
                    });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("starred")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> getStarred([FromQuery] Paging paging)
        {
            try
            {
                var user = await Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, db);
                var books = db.Users.Where(e => e.Id == user.Id)
                            .Include(e => e.WatchList).ThenInclude(e=>e.Category)
                            .Include(r=>r.WatchList).ThenInclude(y=>y.Author)
                            .Select(e => e.WatchList.AsEnumerable().AsQueryable()).FirstOrDefault();
                await books.LoadAsync();

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
                    var filtered = books.Select(b => new GetBooksCustomer
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Price = b.Price,
                        CreatedDate = b.CreatedDate,
                        CategoryName = b.Category.Name,
                        Description = b.Description,
                        Units = b.Units,
                        ImageUrl = b.ImageUrl,
                        Publisher = b.Publisher,
                        Rating = b.FinalRating,
                        UpdateDate = b.UpdatedDate,
                        AuthorId = (int)b.Author.Id,
                        AuthorName = b.Author.Name
                    });
                    var data = await PaginatedList<GetBooksCustomer>.CreateAsync(filtered.AsNoTracking(), paging);
                    return Ok(new
                    {
                        Books = data,
                        TotalPages = data.TotalPages,
                    });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

    }
}
