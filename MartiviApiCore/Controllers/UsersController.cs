﻿using System;
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
using MartiviApi.Models;
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
            if (user == null) return BadRequest("მომხმარებელი ამ სახელით არ მოიძებნა");
            if (model.IsAdmin && user.Type != UserType.Admin) return BadRequest("გთხოვთ გაიაროთ ავტორიზაცია ადმინისტრატორის მომხმარებლით.");
            if (!model.IsAdmin && user.Type != UserType.Client) return BadRequest("გთხოვთ გაიაროთ ავტორიზაცია კლიენტის მომხმარებლით.");
            if (user == null)
                return BadRequest(new { message = "მომხმარებლის სახელი ან პაროლი არასწორია" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
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
            user.Type = UserType.Client;
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
        [AllowAnonymous]
        [HttpPost("registerAdmin")]
        public IActionResult RegisterAdmin([FromBody]RegisterModel model)
        {
            var user = _mapper.Map<User>(model);
            user.Type = UserType.Admin;
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
        [Authorize]
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            int id;
            if (!int.TryParse(User.Identity.Name, out id)) return BadRequest();
            var senderuser = _userService.GetById(id);
            if (senderuser == null) return BadRequest();
            if(senderuser.Type!=UserType.Admin)
            {
                return BadRequest("You dont have access to get data.");
            }

            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            
            var user = _userService.GetById(id);
            return Ok(user);
        }

        [HttpPost("Update")]
        [Authorize]
        public IActionResult Update([FromBody]UpdateModel model)
        {
            // map model to entity and set id

            int id;
            if (!int.TryParse(User.Identity.Name, out id)) return BadRequest();

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
        [Authorize]
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