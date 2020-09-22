using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MaleApiCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        IHubContext<ChatHub> hubContext;
        MaleDbContext maleDbContext;
        public ProductsController(MaleDbContext db, IHubContext<ChatHub> hub)
        {
            hubContext = hub;
            maleDbContext = db;
        }
        [Route("{AddProductToCategoryId}")]
        [HttpPost]
        public IActionResult Post(int AddProductToCategoryId, Product product)
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (product.ProductId > 0)
            {
                var existedProduct = maleDbContext.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
                if (existedProduct != null)
                {
                    existedProduct.Description = product.Description;
                    existedProduct.Image = product.Image;
                    existedProduct.Name = product.Name;
                    existedProduct.Price = product.Price;
                    existedProduct.QuantityInSupply = product.QuantityInSupply;
                    existedProduct.Weight = product.Weight;

                    maleDbContext.SaveChanges();
                    hubContext.Clients.All.SendAsync("UpdateListing");
                    return StatusCode(StatusCodes.Status201Created);
                }
                else
                {
                    return BadRequest("პროდუქტი არ მოიძებნა");
                }
            }

            var res = maleDbContext.Categories.Include("Products").Single(c => c.CategoryId == AddProductToCategoryId);
            product.CategoryId = AddProductToCategoryId;
            //product.CategoryId = res.CategoryId;
            res.Products.Add(product);

            maleDbContext.Database.OpenConnection();
            try
            {

                maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] ON");
                maleDbContext.SaveChanges();
                hubContext.Clients.All.SendAsync("UpdateListing");
                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] OFF");
                maleDbContext.Database.CloseConnection();
            }


        }


        [HttpPost]
        public IActionResult Post(Category category)
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (category.CategoryId > 0)
            {
                try
                {
                    var currentCat = maleDbContext.Categories.FirstOrDefault(cat => cat.CategoryId == category.CategoryId);
                    if (currentCat != null)
                    {
                        currentCat.Name = category.Name;
                        currentCat.Image = category.Image;
                        maleDbContext.SaveChanges();
                        hubContext.Clients.All.SendAsync("UpdateListing");
                        return StatusCode(StatusCodes.Status201Created);
                    }
                }
                catch
                {

                }
            }
            int maxid;
            if (maleDbContext.Categories.Count() == 0) maxid = 0;
            else maxid = maleDbContext.Categories.Max(c => c.CategoryId);
            category.CategoryId = maxid + 1;
            maleDbContext.Categories.Add(category);
            maleDbContext.Database.OpenConnection();
            try
            {
                maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] ON");
                maleDbContext.SaveChanges();
                hubContext.Clients.All.SendAsync("UpdateListing");
                return StatusCode(StatusCodes.Status201Created);
            }
            finally
            {
                maleDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Categories] OFF");
                maleDbContext.Database.CloseConnection();
            }



        }


        [HttpGet]
        [Route("Delete/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");
            var p = maleDbContext.Products.FirstOrDefault(p => p.ProductId == id);
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
                maleDbContext.Products.Remove(p);
                maleDbContext.SaveChanges();
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