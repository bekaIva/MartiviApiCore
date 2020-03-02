using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MartiviApi.Data;
using MartiviApiCore.Chathub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
        [Route("delete/{id}")]
        [HttpGet]
        public IActionResult DeleteCategorie(int id)
        {
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
