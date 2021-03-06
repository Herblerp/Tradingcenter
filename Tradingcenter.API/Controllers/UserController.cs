﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Trainingcenter.Domain.DTOs.UserDTOs;
using Trainingcenter.Domain.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;

//DONE
namespace Tradingcenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        #region DependecyInjection

        private readonly IUserServices _userService;
        public IConfiguration _config;
        private HtmlEncoder _htmlEncoder;
        private JavaScriptEncoder _javaScriptEncoder;
        private UrlEncoder _urlEncoder;

        public UserController(  IUserServices userService, 
                                IConfiguration config, 
                                HtmlEncoder htmlEncoder,
                                JavaScriptEncoder javascriptEncoder,
                                UrlEncoder urlEncoder)
        {
            _userService = userService;
            _config = config;
            _htmlEncoder = htmlEncoder;
            _javaScriptEncoder = javascriptEncoder;
            _urlEncoder = urlEncoder;
        }

        #endregion

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserToLoginDTO userToLogin)
        {
            try
            {
                var user = await _userService.Login(userToLogin);

                if (user == null)
                {
                    //Unauthorized
                    return StatusCode(401, "Bad login");
                }
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                    //More claims here
                };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Appsettings:Token").Value));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenhandler = new JwtSecurityTokenHandler();

                var token = tokenhandler.CreateToken(tokenDescriptor);

                //Ok
                return StatusCode(200, new { token = tokenhandler.WriteToken(token) });
            }
            catch
            {
                return StatusCode(500, "Something went wrong while attempting to log in, please try again in a few moments.");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserToRegisterDTO userToRegister)
        {
            try
            {
                userToRegister.Email = _htmlEncoder.Encode(_javaScriptEncoder.Encode(userToRegister.Email));
                userToRegister.Username = _htmlEncoder.Encode(_javaScriptEncoder.Encode(userToRegister.Username));

                if (await _userService.UserExists(userToRegister.Username))
                {
                    return StatusCode(400, "Username already taken");
                }
                if (!_userService.IsValidEmail(userToRegister.Email))
                {
                    return StatusCode(400, "Invalid email");
                }
                var user = await _userService.Register(userToRegister);
                return StatusCode(200, user);
            }
            catch
            {
                return StatusCode(500, "Something went wrong while attempting to register, please try again in a few moments.");
            }
        }

        [HttpPost("validate/{key}")]
        public async Task<IActionResult> Validate(string key)
        {

            var user = await _userService.ValidateUser(key);

            if(user == null)
               return StatusCode(400, "Failed to verify email");

            return StatusCode(200);

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(UserToUpdateDTO userToUpdate)
        {
            userToUpdate.FirstName = _javaScriptEncoder.Encode(_htmlEncoder.Encode(userToUpdate.FirstName));
            userToUpdate.LastName = _javaScriptEncoder.Encode(_htmlEncoder.Encode(userToUpdate.LastName));
            userToUpdate.Phone = _javaScriptEncoder.Encode(_htmlEncoder.Encode(userToUpdate.Phone));
            userToUpdate.Email = _javaScriptEncoder.Encode(_htmlEncoder.Encode(userToUpdate.Email));
            userToUpdate.Username = _htmlEncoder.Encode(_javaScriptEncoder.Encode(userToUpdate.Username));
            userToUpdate.Description = _htmlEncoder.Encode(_javaScriptEncoder.Encode(userToUpdate.Description));

           if (userToUpdate.Email != null && !_userService.IsValidEmail(userToUpdate.Email))
            {
                return StatusCode(400, "Thats not an email address...");
            }

            int userId = Int32.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (await _userService.UpdateUser(userToUpdate, userId) == null)
            {
                return StatusCode(500, "kaka");
            }
            return StatusCode(200);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(int userId)
        {
            if (userId != 0)
            {
                var user = await _userService.GetPublicUserById(userId);
                if (user == null)
                {
                    return StatusCode(400, "User with id " + userId + " was not found.");
                }
                return StatusCode(200, user);
            }
            else
            {
                userId = Int32.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userService.GetUserById(userId);
                if (user == null)
                {
                    return StatusCode(400, "User with id " + userId + " was not found.");
                }
                return StatusCode(200, user);
            }
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUser(string username)
        {
            if(username == null)
            {
                return StatusCode(400, "Please specify a username");
            }
            return StatusCode(200, await _userService.SearchUser(username));
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(UserToLoginDTO user)
        {
            var userId = Int32.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _userService.DeleteUser(userId, user.Password);
            return StatusCode(200);
        }
    }
}