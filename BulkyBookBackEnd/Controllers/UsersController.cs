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
        public async Task<IActionResult> GetAllCustomers([FromQuery] Paging paging, [FromQuery] string? search)
        {
            var users = from u in db.Users
                        where u.Role=="Customer"
                        select u;
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
            var user = Jwt.findUserByToken(HttpContext.User.Identity as ClaimsIdentity, db);
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Customer")]
        public async Task<IActionResult> CreateAdmin(CreateUser createUser)
        {
            try
            {
                if (createUser.Role != "Admin")
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
        [Route("login")]
        [Produces("application/json")]

        public async Task<IActionResult> Login(UserLogin login)
        {
            try
            {
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

    }
}
