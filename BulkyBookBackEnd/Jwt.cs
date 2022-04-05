using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BulkyBookBackEnd
{
    public static class Jwt
    {
        public static string Key { get; set; }
        public static string Issuer { get; set; }
        public static string Audience { get; set; }

        public static Claim[] generateClaims(User user)
        {
            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.UserName),
                    new Claim(ClaimTypes.Email,user.EmailAddress),
                    new Claim(ClaimTypes.Role,user.Role),
                    new Claim(ClaimTypes.GivenName,user.FirstName),
                    new Claim(ClaimTypes.Surname,user.LastName),
                };

            return claims;
        }

        public static JwtSecurityToken generateToken(Claim[] claims)
        {
            var token = new JwtSecurityToken
                    (
                        issuer: Jwt.Issuer,
                        audience: Jwt.Audience,
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(10),
                        notBefore: DateTime.UtcNow,
                        signingCredentials: new SigningCredentials(
                                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.Key)),
                                SecurityAlgorithms.HmacSha256
                            )
                    );
            return token;
        }

        public static string generateTokenString(JwtSecurityToken jwtSecurityToken)
        {
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return tokenString.ToString();
        }

        public static IEnumerable<Claim> decodeToken(ClaimsIdentity claimsIdentity)
        {
            try
            {
                IEnumerable<Claim> claims = claimsIdentity.Claims;
                return claims;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<User> findUserByToken(ClaimsIdentity claimsIdentity,BookDbContext _context)
        {
            try
            {
                var claims = Jwt.decodeToken(claimsIdentity);
                var role = claims.Where(x => x.Type == ClaimTypes.Role).FirstOrDefault().Value;
                var email = claims.Where(y => y.Type == ClaimTypes.Email).FirstOrDefault().Value;
                var user = await (from u in _context.Users
                                  where u.EmailAddress == email
                                  select u).FirstOrDefaultAsync();
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
