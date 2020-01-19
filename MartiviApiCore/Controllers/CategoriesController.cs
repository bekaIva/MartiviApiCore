using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MartiviApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        MartiviDbContext martiviDbContext;

        public CategoriesController(MartiviDbContext db)
        {
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
    }
}
