using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MartiviApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenusController : ControllerBase
    {
        MartiviDbContext martiviDbContext;

        public MenusController(MartiviDbContext db)
        {
            martiviDbContext = db;
        }

        [HttpGet]
        public IActionResult GetMenus()
        {
            var menus = martiviDbContext.Menus.Include("SubMenus");
            return Ok(menus);
        }
        
        [Route("id/{id}")]
        [HttpGet]
        public IActionResult GetMenu(int id)
        {
            var menu = martiviDbContext.Menus.Include("SubMenus").FirstOrDefault(m=>m.Id==id);
            if (menu==null)
            {
                return NotFound();
            }

            return Ok(menu);
        }
    }
}
