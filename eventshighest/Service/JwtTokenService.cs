using eventshighest.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }
        public Token CreateToken(AppUser appUser)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,appUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            //create a key from our private key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //create token
            int expiryInMinutes = Convert.ToInt32(_config["Jwt:ExpireTime"]);
            var token = new JwtSecurityToken(
              issuer: _config["Jwt:Issuer"],
              audience: _config["Jwt:Audience"],
               claims,
              expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
              signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            Token tokens = new Token();
            tokens.token = new JwtSecurityTokenHandler().WriteToken(token);
            tokens.expiration = token.ValidTo;
            return tokens;
        }
    }
    public class Token
    {
        public string token { get; set; }
        public DateTime expiration { get; set; }
    }
}
