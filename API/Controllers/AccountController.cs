using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        public readonly ITokenService _TokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _TokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Rigester(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.Username)) return BadRequest("User is taken");

            using var hmac = new HMACSHA512();
            var User = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = User.UserName,
                Token = _TokenService.CreateToken(User)
            };
        }

        private async Task<bool> UserExist(string Username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == Username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> login(LoginDto loginDto)
        {
            var User = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
            if (User == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(User.PasswordSalt);

            var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < ComputedHash.Length; i++)
            {
                if (ComputedHash[i] != User.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
            return new UserDto
            {
                Username = User.UserName,
                Token = _TokenService.CreateToken(User)
            };
        }



    }
}