using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MaleApi.Data;
using MaleApi.Models.Users;
using MaleApi.Services;
using MaleApiCore.Helpers;
using MaleApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using MaleApiCore.Chathub;
using MaleApiCore.Models.Users;
using MaleApiCore.Models;
using System.Security.Cryptography;

namespace MaleApiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly EmailConfiguration _emailConfiguration;
        IHubContext<ChatHub> _hub;
        MaleDbContext maleDbContext;
        public UsersController(MaleDbContext db,
            IUserService userService,
            IMapper mapper,
            IConfiguration config, IHubContext<ChatHub> hub)
        {
            maleDbContext = db;
            _hub = hub;
            _userService = userService;
            _mapper = mapper;
            _appSettings = config.GetSection("AppSettings").Get<AppSettings>();
            _emailConfiguration = config.GetSection("EmailConfiguration").Get<EmailConfiguration>();
        }

        [HttpPost("RequestPasswordRecoveryCode")]
        public IActionResult RequestPasswordRecoveryCode(RequestPasswordRecoveryCodeModel email)
        {
            try
            {
                EmailSender _emailSender = new EmailSender(_emailConfiguration); 
                var user = maleDbContext.Users.FirstOrDefault(u => u.Username.ToLower() == email.Username.ToLower());
                if (user == null) 
                {
                    return Ok(new PasswordChangeResult() { Error = Result.UserNotFound, Message = "მომხმარებელი არ მოიძებნა" });
                }
                
                Random r = new Random();
                int Code = r.Next(100000, 999999);
                string hash = CalculateMD5Hash(_appSettings.HashSecret + HashToHex(user.PasswordHash) + Code.ToString() + user.Username);
                maleDbContext.PasswordChangeStores.Add(new PasswordChangeStore() { Code = Code, Hash = hash,PasswordTime=DateTime.Now.Ticks });
                maleDbContext.SaveChanges();
                var message = new Message(new string[] { email.Username }, "პაროლის აღდგენა", "გთხოვთ გამოიყენოთ ეს კოდი პაროლის აღდგენისთვის: " + Code.ToString());
                _emailSender.SendEmail(message);
                return Ok(new PasswordChangeResult() { Error = Result.CodeSent, Message = "აღდგენის კოდი გაგზავნილია." });
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }
        }

        [HttpPost("RecoverPassword")]
        public IActionResult RecoverPassword(PasswordChangeRequestModel changeRequest)
        {
            PasswordChangeResult result = new PasswordChangeResult();
            try
            {

                var passwordChange = maleDbContext.PasswordChangeStores.FirstOrDefault(pcs => pcs.Code == changeRequest.Code);



                if (passwordChange == null)
                {
                    return Ok(new PasswordChangeResult() { Error = Result.InvalidCode, Message = "აღდგენის კოდი არასწორია" });
                }
                var currentTick = DateTime.Now.Ticks;
                var dif = ((currentTick - passwordChange.PasswordTime) / 10000000) / 60;
                if (dif > 15)
                {
                    return Ok(new PasswordChangeResult() { Error = Result.CodeOutOfDated, Message = "აღდგენის კოდი ვადაგასულია" });
                }
                var user = maleDbContext.Users.FirstOrDefault(u => u.Username.ToLower() == changeRequest.Username.ToLower());
                if (user == null)
                {
                    return Ok(new PasswordChangeResult() { Error = Result.UserNotFound, Message = "მომხმარებელი არ მოიძებნა" });
                }
            

                string hash = CalculateMD5Hash(_appSettings.HashSecret + HashToHex(user.PasswordHash) + changeRequest.Code + changeRequest.Username);
                if (!hash.Equals(passwordChange.Hash))
                {
                    return Ok(new PasswordChangeResult() { Error = Result.InvalidCode, Message = "აღდგენის კოდი არასწორია" });
                }
                _userService.Update(user, changeRequest.NewPassword);
                maleDbContext.PasswordChangeStores.Remove(passwordChange);
                maleDbContext.SaveChanges();
                EmailSender _emailSender = new EmailSender(_emailConfiguration);
                var message = new Message(new string[] { user.Username }, "პაროლის ცვლილება", user.Username + " მომხმარებელს შეეცვალა პაროლი");
                _emailSender.SendEmail(message);
                return Ok(new PasswordChangeResult() { Error = Result.PasswordChanged, Message = "პაროლი შეიცვალა." });
            }
            catch (Exception ee)
            {
                return BadRequest(new PasswordChangeResult() { Error = Result.UnknownError, Message = ee.Message });
            }
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
                Type = user.Type,
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

                var admins = maleDbContext.Users.AsQueryable().Where(user => user.Type == UserType.Admin);
                foreach (var admin in admins)
                {
                    _hub.Clients.User(admin.UserId.ToString()).SendAsync("UpdateUsers");
                }

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


        [HttpGet("GetAdresses")]
        [Authorize]
        public IActionResult GetAdresses()
        {

            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest("no user id" + User.Identity.Name);

            var user = maleDbContext.Users.Include(u=>u.UserAddresses).ThenInclude(addreess=>addreess.Coordinates).FirstOrDefault(user => user.UserId == userid);
            return Ok(user.UserAddresses);
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

        [HttpPost("AddAddress")]
        [Authorize]
        public IActionResult AddAddress(UserAddress address)
        {
            // map model to entity and set id

            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest("no user id" + User.Identity.Name);

            var user = maleDbContext.Users.Include("UserAddresses").FirstOrDefault(user => user.UserId == userid);
            

            try
            {
                user.UserAddresses.Add(address);
                maleDbContext.SaveChanges();
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("RemoveAddress")]
        [Authorize]
        public IActionResult RemoveAddress(UserAddress address)
        {
            // map model to entity and set id

            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest("no user id" + User.Identity.Name);

            var user = maleDbContext.Users.Include(usr=>usr.UserAddresses).ThenInclude(uadr=>uadr.Coordinates).FirstOrDefault(user => user.UserId == userid);


            try
            {
                
                var ua = user.UserAddresses.FirstOrDefault(a => a.UserAddressId == address.UserAddressId);
                maleDbContext.UserAddresses.Remove(ua);
                maleDbContext.SaveChanges();
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
            try
            {
                _userService.Delete(id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        string HashToHex(byte[] hash)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2").ToLower());
            }
            return sb.ToString().ToLower();
        }
        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            return HashToHex(hash);           
        }
    }

    //[Route("api/[controller]")]
    //[ApiController]
    //public class UsersController : ControllerBase
    //{
    //    MaleDbContext maleDbContext;
    //    public UsersController(MaleDbContext db)
    //    {
    //        maleDbContext = db;
    //    }

    //    [HttpPost]
    //    public IActionResult Post(User user)
    //    {

    //        int maxid;
    //        if (maleDbContext.Users.Count() == 0) maxid = 0;
    //        else maxid = maleDbContext.Users.Max(u => u.UserId);
    //        user.UserId = maxid + 1;
    //        maleDbContext.Users.Add(user);
    //        maleDbContext.Database.OpenConnection();
    //        try
    //        {
    //            maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] ON");
    //            maleDbContext.SaveChanges();

    //            return StatusCode(StatusCodes.Status201Created);
    //        }
    //        finally
    //        {
    //            maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] OFF");
    //            maleDbContext.Database.CloseConnection();
    //        }
    //    }
    //}
}