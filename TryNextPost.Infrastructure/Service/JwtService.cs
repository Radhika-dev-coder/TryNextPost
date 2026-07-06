using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.IServices.Interface;

namespace TryNextPost.Infrastructure.Service
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        static JwtService()
        {
            
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }
        public JwtService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateOtpToken(string email, string otp, DateTime expiry)
        {
            var claims = new[]
            {
                new Claim("email", email),
                new Claim("otp", otp)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:CongigureMinutes"])),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public string GenerateToken(string userId, string email)
        {
            var jwtKey = _config["Jwt:Key"];
            Console.WriteLine("JWT KEY: " + (jwtKey ?? "NULL"));
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (bool isValid, string email) ValidateOtpToken(string token, string enteredOtp)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = key
                }, out _);

                var emailClaim = principal.FindFirst("email")?.Value;
                var otpClaim = principal.FindFirst("otp")?.Value;

                if (otpClaim == enteredOtp)
                    return (true, emailClaim);

                return (false, null);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
