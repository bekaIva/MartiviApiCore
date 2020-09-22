using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MartiviApi.Data;
using MartiviApi.Models;
using MartiviApiCore.Chathub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        MartiviDbContext martiviDbContext;
        IHubContext<ChatHub> hubContext;
        public CategoriesController(MartiviDbContext db, IHubContext<ChatHub> hub)
        {
            hubContext = hub;
            martiviDbContext = db;
        }

        
       

        [HttpGet]
        public IActionResult GetCategories()
        {

         

            var categories = martiviDbContext.Categories.Include("Products");
            return Ok(categories);
        }
        [Route("GetFilteredCategories/")]
        [HttpGet]
        public IActionResult GetFilteredCategories()
        {
            var categories = martiviDbContext.Categories.IncludeFilter(c => c.Products.Where(p=>p.QuantityInSupply>0));
            return Ok(categories);
        }


        [Route("id/{id}")]
        [HttpGet]
        public IActionResult GetCategories(int id)
        {
          

            var categories = martiviDbContext.Categories.Include("Products").FirstOrDefault(c=>c.CategoryId==id);
            if (categories == null)
            {
                return NotFound();
            }

            return Ok(categories);
        }

        [Authorize]
        [Route("delete/{id}")]
        [HttpGet]
        public IActionResult DeleteCategorie(int id)
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = martiviDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");
            var category = martiviDbContext.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            if (category != null)
            {
                try
                {
                    var match = Regex.Match(category.Image, "Images/(?<image>.*?.image)");
                    if (match.Groups["image"]?.Success ?? false)
                    {
                        System.IO.File.Delete("Images/" + match.Groups["image"].Value);
                    }
                }
                catch
                {

                }

                category.Products.Clear();
                martiviDbContext.SaveChanges();
                martiviDbContext.Categories.Remove(category);
                martiviDbContext.SaveChanges();
                hubContext.Clients.All.SendAsync("UpdateListing");
                return StatusCode(StatusCodes.Status200OK);
            }
            return NotFound();
        }
    }
}
