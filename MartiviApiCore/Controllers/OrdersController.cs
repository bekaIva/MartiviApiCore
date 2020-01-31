using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MartiviApi.Data;
using MartiviApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
      
        MartiviDbContext martiviDbContext;

        public OrdersController(MartiviDbContext db)
        {
            martiviDbContext = db;
        }

        [HttpPost]
        [Route("DeleteOrder/")]
        public IActionResult PostDeleteOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            order = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);
            order.User = null;
           

            foreach(var p in order.OrderedProducts)
            {
                martiviDbContext.Remove(p);
            }
            order.OrderedProducts.Clear();
            martiviDbContext.SaveChanges();

            

            martiviDbContext.Orders.Remove(order);
            martiviDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);


        }

        [HttpPost]
        [Route("CancelOrder/")]
        public IActionResult PostCancelOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            order = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);

            order.Status = OrderStatus.Canceled;

           
            martiviDbContext.SaveChanges();


            return StatusCode(StatusCodes.Status201Created);


        }

        [HttpPost]
        public IActionResult Post(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = martiviDbContext.Users.FirstOrDefault(u => u.UserId == order.User.UserId);
            order.User = user;
            martiviDbContext.Orders.Add(order);
            martiviDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);


        }

        [Route("GetOrders/")]
        [HttpPost]
        public IActionResult GetOrders(User user)
        {
            var orders = martiviDbContext.Orders.Include("OrderedProducts").Where(o => o.User.UserId == user.UserId);
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }
    }
}
