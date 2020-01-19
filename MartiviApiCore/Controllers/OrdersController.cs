using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MartiviApi.Data;
using MartiviApi.Models;
using MartiviApiCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
      
        MartiviDbContext martiviDbContext;

        public OrdersController(MartiviDbContext db)
        {
            martiviDbContext = db;
        }

        [HttpPost]
        public IActionResult Post(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            martiviDbContext.Orders.Add(order);
            martiviDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        [Route("GetOrders/")]
        [HttpPost]
        public IActionResult GetOrders(User user)
        {
            var orders = martiviDbContext.Orders.FirstOrDefault(o => o.User.UserId == user.UserId);
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }
    }
}
