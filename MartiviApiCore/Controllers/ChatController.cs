using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MartiviApi.Data;
using MartiviApi.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MartiviApiCore.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ChatController : ControllerBase
    {
        MartiviDbContext martiviDbContext;
        public ChatController(MartiviDbContext db)
        {
            martiviDbContext = db;
        }
        [HttpPost]
        public IActionResult Post(ChatMessage message)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = martiviDbContext.Users.Include("Messages").Single(c => c.UserId == message.UserId);
            //product.CategoryId = res.CategoryId;
            res.Messages.Add(message);

            martiviDbContext.Database.OpenConnection();
            try
            {
                
                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] ON");
                martiviDbContext.SaveChanges();

                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] OFF");
                martiviDbContext.Database.CloseConnection();
            }


        }

        [Route("GetChat")]
        [HttpGet]
        public IActionResult GetChat()
        {
            int id;
            if (!int.TryParse(User.Identity.Name, out id)) return BadRequest();
            var user = martiviDbContext.Users.Include("Messages").FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.Messages);
        }
    }
}