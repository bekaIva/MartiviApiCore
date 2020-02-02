using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MartiviApi.Data;
using MartiviApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MartiviApiCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        MartiviDbContext martiviDbContext;
        public ProductsController(MartiviDbContext db)
        {
            martiviDbContext = db;
        }
        [Route("{AddProductToCategoryId}")]
        [HttpPost]
        public IActionResult Post(int AddProductToCategoryId, Product product)
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = martiviDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = martiviDbContext.Categories.Include("Products").Single(c => c.CategoryId == AddProductToCategoryId);
            product.CategoryId = AddProductToCategoryId;
            //product.CategoryId = res.CategoryId;
            res.Products.Add(product);

            martiviDbContext.Database.OpenConnection();
            try
            {

                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] ON");
                martiviDbContext.SaveChanges();
               
                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] OFF");
                martiviDbContext.Database.CloseConnection();
            }

            
        }

        
        [HttpPost]
        public IActionResult Post(Category category)
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = martiviDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int maxid;
            if (martiviDbContext.Categories.Count() == 0) maxid = 0;
            else maxid = martiviDbContext.Categories.Max(c => c.CategoryId);
            category.CategoryId = maxid + 1;
            martiviDbContext.Categories.Add(category);
            martiviDbContext.Database.OpenConnection();
            try
            {
                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] ON");
                martiviDbContext.SaveChanges();
               
                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] OFF");
                martiviDbContext.Database.CloseConnection();
            }
          
           
            
        }
    }

}