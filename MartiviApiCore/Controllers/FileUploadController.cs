using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MartiviApi.Data;
using MartiviApi.Models;
using MartiviApiCore.Chathub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MartiviApiCore.Controllers
{
    
    [ApiController]    
    public class UploadController : ControllerBase
    {
        // GET: api/FileUpload
        MartiviDbContext martiviDbContext;
        IMapper mapper;
        IHubContext<ChatHub> hubContext;
        public UploadController(MartiviDbContext db, IMapper mapper, IHubContext<ChatHub> hub)
        {
            martiviDbContext = db;
            this.mapper = mapper;
            hubContext = hub;
        }

        [Authorize]
        [HttpPost]
        [Route("api/Upload")]
        public IActionResult Upload(IFormFile file)
        {
            // Extract file name from whatever was posted by browser

            // If file with same name exists delete it
            try
            {
                int userid;
                if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest("no user id"+ User.Identity.Name);
                var user = martiviDbContext.Users.FirstOrDefault(user => user.UserId == userid);
                if (user==null) return BadRequest("არაავტორიზებული მომხმარებელი");

                if (!Directory.Exists("Images"))
                {
                    Directory.CreateDirectory("Images");
                }
                string filename = file.Name + Guid.NewGuid().ToString()+".image";

                // Create new local file and copy contents of uploaded file
                using (var localFile = System.IO.File.OpenWrite("Images/" + filename))
                using (var uploadedFile = file.OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                    return Ok(filename);

                 
                }
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }


        }
        [HttpGet("Images/{link}")]
        public IActionResult GetBlobDownload(string link)
        {
            return File(System.IO.File.Open("Images/" + link, FileMode.Open), "application/force-download");
        }
      
    }
}
