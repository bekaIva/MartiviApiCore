using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MartiviApi.Data;
using MartiviApi.Models.Users;
using MartiviApi.Services;
using MartiviApiCore.Helpers;
using MartiviApiCore.Models;
using MartiviApiCore.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MartiviApiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IConfiguration config)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = config.GetSection("AppSettings").Get<AppSettings>();
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                UserId = user.UserId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            var user = _mapper.Map<User>(model);
            try
            {
                var u = _userService.Create(user, model.Password);
                return Ok(u);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var model = _mapper.Map<IList<UserModel>>(users);
            return Ok(model);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            var model = _mapper.Map<UserModel>(user);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UpdateModel model)
        {
            // map model to entity and set id
            var user = _mapper.Map<User>(model);
            user.UserId = id;

            try
            {
                // update user 
                _userService.Update(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }
    }

    //[Route("api/[controller]")]
    //[ApiController]
    //public class UsersController : ControllerBase
    //{
    //    MartiviDbContext martiviDbContext;
    //    public UsersController(MartiviDbContext db)
    //    {
    //        martiviDbContext = db;
    //    }

    //    [HttpPost]
    //    public IActionResult Post(User user)
    //    {

    //        int maxid;
    //        if (martiviDbContext.Users.Count() == 0) maxid = 0;
    //        else maxid = martiviDbContext.Users.Max(u => u.UserId);
    //        user.UserId = maxid + 1;
    //        martiviDbContext.Users.Add(user);
    //        martiviDbContext.Database.OpenConnection();
    //        try
    //        {
    //            martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] ON");
    //            martiviDbContext.SaveChanges();

    //            return StatusCode(StatusCodes.Status201Created);
    //        }
    //        finally
    //        {
    //            martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] OFF");
    //            martiviDbContext.Database.CloseConnection();
    //        }
    //    }
    //}
}