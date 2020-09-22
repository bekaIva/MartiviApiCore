using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaleApi.Data;
using MaleApi.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaleApiCore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ChatController : ControllerBase
    {
        MaleDbContext maleDbContext;
        public ChatController(MaleDbContext db)
        {
            maleDbContext = db;
        }
        [HttpPost]
        public IActionResult Post(ChatMessage message)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = maleDbContext.Users.Include("Messages").Single(c => c.UserId == message.UserId);
            //product.CategoryId = res.CategoryId;
            res.Messages.Add(message);

            maleDbContext.Database.OpenConnection();
            try
            {
                
                maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] ON");
                maleDbContext.SaveChanges();

                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] OFF");
                maleDbContext.Database.CloseConnection();
            }


        }

        [Route("GetChat")]
        [HttpGet]
        public IActionResult GetChat()
        {
            int id;
            if (!int.TryParse(User.Identity.Name, out id)) return BadRequest();
            var user = maleDbContext.Users.Include("Messages").FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.Messages);
        }
    }
}