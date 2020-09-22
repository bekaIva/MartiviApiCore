using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MaleApi.Data;
using MaleApi.Models;
using MaleApiCore.Chathub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace MaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        MaleDbContext maleDbContext;
        IHubContext<ChatHub> hubContext;
        public CategoriesController(MaleDbContext db, IHubContext<ChatHub> hub)
        {
            hubContext = hub;
            maleDbContext = db;
        }

        
       

        [HttpGet]
        public IActionResult GetCategories()
        {

         

            var categories = maleDbContext.Categories.Include("Products");
            return Ok(categories);
        }
        [Route("GetFilteredCategories/")]
        [HttpGet]
        public IActionResult GetFilteredCategories()
        {
            var categories = maleDbContext.Categories.IncludeFilter(c => c.Products.Where(p=>p.QuantityInSupply>0));
            return Ok(categories);
        }


        [Route("id/{id}")]
        [HttpGet]
        public IActionResult GetCategories(int id)
        {
          

            var categories = maleDbContext.Categories.Include("Products").FirstOrDefault(c=>c.CategoryId==id);
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
            var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");
            var category = maleDbContext.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
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
                maleDbContext.SaveChanges();
                maleDbContext.Categories.Remove(category);
                maleDbContext.SaveChanges();
                hubContext.Clients.All.SendAsync("UpdateListing");
                return StatusCode(StatusCodes.Status200OK);
            }
            return NotFound();
        }
    }
}
