using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MartiviApiCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        IHubContext<ChatHub> hubContext;
        MartiviDbContext martiviDbContext;
        public ProductsController(MartiviDbContext db, IHubContext<ChatHub> hub)
        {
            hubContext = hub;
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
            if (product.ProductId > 0)
            {
                var existedProduct = martiviDbContext.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
                if (existedProduct != null)
                {
                    existedProduct.Description = product.Description;
                    existedProduct.Image = product.Image;
                    existedProduct.Name = product.Name;
                    existedProduct.Price = product.Price;
                    existedProduct.QuantityInSupply = product.QuantityInSupply;
                    existedProduct.Weight = product.Weight;

                    martiviDbContext.SaveChanges();
                    hubContext.Clients.All.SendAsync("UpdateListing");
                    return StatusCode(StatusCodes.Status201Created);
                }
                else
                {
                    return BadRequest("პროდუქტი არ მოიძებნა");
                }
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
                hubContext.Clients.All.SendAsync("UpdateListing");
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

            if (category.CategoryId > 0)
            {
                try
                {
                    var currentCat = martiviDbContext.Categories.FirstOrDefault(cat => cat.CategoryId == category.CategoryId);
                    if (currentCat != null)
                    {
                        currentCat.Name = category.Name;
                        currentCat.Image = category.Image;
                        martiviDbContext.SaveChanges();
                        hubContext.Clients.All.SendAsync("UpdateListing");
                        return StatusCode(StatusCodes.Status201Created);
                    }
                }
                catch
                {

                }
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
                hubContext.Clients.All.SendAsync("UpdateListing");
                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] OFF");
                martiviDbContext.Database.CloseConnection();
            }



        }


        //http://martivi.net/api/upload/Images/file110e635d-d1b7-4d02-abd1-241c90a9b945
        [HttpGet]
        [Route("Delete/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var p = martiviDbContext.Products.FirstOrDefault(p => p.ProductId == id);
            if (p != null)
            {
                try
                {
                    var match = Regex.Match(p.Image, "Images/(?<image>.*?.image)");
                    if (match.Groups["image"]?.Success ?? false)
                    {
                        System.IO.File.Delete("Images/"+match.Groups["image"].Value);
                    }
                }
                catch
                {

                }
                martiviDbContext.Products.Remove(p);
                martiviDbContext.SaveChanges();
                hubContext.Clients.All.SendAsync("UpdateListing");
                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                return BadRequest("პროდუქტი არ მოიძებნა");
            }
        }
    }

}