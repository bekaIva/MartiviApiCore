using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AutoMapper;
using MartiviApi.Data;
using MartiviApi.Models;
using MartiviApiCore.Chathub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
      
        MartiviDbContext martiviDbContext;
        IMapper mapper;
        IHubContext<ChatHub> hubContext;
        public OrdersController(MartiviDbContext db, IMapper mapper, IHubContext<ChatHub> hub)
        {
            martiviDbContext = db;
            this.mapper = mapper;
            hubContext = hub;
        }

        [HttpPost]
        [Route("DeleteOrder/")]
        public IActionResult PostDeleteOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);
            exsistingOrder.User = null;
           

            foreach(var p in exsistingOrder.OrderedProducts)
            {
                martiviDbContext.Remove(p);
            }
            exsistingOrder.OrderedProducts.Clear();
            martiviDbContext.SaveChanges();

            

            martiviDbContext.Orders.Remove(exsistingOrder);
            martiviDbContext.SaveChanges();

           var admins = martiviDbContext.Users.Where(user => user.Type == UserType.Admin);
            foreach(var admin in admins)
            {
                hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrderListing");
            }

            hubContext.Clients.All.SendAsync("UpdateListing");
            hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrderListing");
            hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrderListing"); ;
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
            var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);

            exsistingOrder.Status = OrderStatus.Canceled;
            var canceledorder = mapper.Map<CanceledOrder>(exsistingOrder);
            martiviDbContext.CanceledOrders.Add(canceledorder);

            foreach (var p in exsistingOrder.OrderedProducts)
            {
                var res = martiviDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
                if (res != null)
                {
                    res.QuantityInSupply += p.Quantity;
                }
            }

            martiviDbContext.SaveChanges();
            var admins = martiviDbContext.Users.Where(user => user.Type == UserType.Admin);
            foreach (var admin in admins)
            {
                hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
            }

            hubContext.Clients.All.SendAsync("UpdateListing");
            hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
            hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrder", exsistingOrder);
            return StatusCode(StatusCodes.Status201Created);






        }

        [HttpPost]
        [Route("CompleteOrder/")]
        public IActionResult PostCompleteOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);

            exsistingOrder.Status = OrderStatus.Completed;
            var CompletedOrder = mapper.Map<CompletedOrder>(exsistingOrder);
            martiviDbContext.CompletedOrders.Add(CompletedOrder);

            foreach (var p in exsistingOrder.OrderedProducts)
            {
                var res = martiviDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
                if (res != null)
                {
                    res.QuantityInSupply += p.Quantity;
                }
            }

            martiviDbContext.SaveChanges();
            hubContext.Clients.All.SendAsync("UpdateListing");
            hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
            hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrder", exsistingOrder);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [Route("SetOrderStatus/")]
        public IActionResult PostSetOrderStatus(Order order)
        {
           



            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);
            if (exsistingOrder.Status == order.Status) return Ok();
            exsistingOrder.Status = order.Status;

            switch (order.Status)
            {
                case OrderStatus.Accepted:
                    {
                        var canceled = martiviDbContext.CanceledOrders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == exsistingOrder.OrderId);
                        if (canceled != null) martiviDbContext.CanceledOrders.Remove(canceled);

                        var completed = martiviDbContext.CompletedOrders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == exsistingOrder.OrderId);
                        if (completed != null) martiviDbContext.CompletedOrders.Remove(completed);

                        martiviDbContext.SaveChanges();
                        break;
                    }
                case OrderStatus.Canceled:
                    {
                        var canceled = martiviDbContext.CanceledOrders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == exsistingOrder.OrderId);
                        if (canceled == null) martiviDbContext.CanceledOrders.Add(mapper.Map<CanceledOrder>(exsistingOrder));

                        var completed = martiviDbContext.CompletedOrders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == exsistingOrder.OrderId);
                        if (completed != null) martiviDbContext.CompletedOrders.Remove(completed);

                        martiviDbContext.SaveChanges();
                        break;
                    }
                case OrderStatus.Completed:
                    {
                        var canceled = martiviDbContext.CanceledOrders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == exsistingOrder.OrderId);
                        if (canceled != null) martiviDbContext.CanceledOrders.Remove(canceled);

                        var completed = martiviDbContext.CompletedOrders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == exsistingOrder.OrderId);
                        if (completed == null) martiviDbContext.CompletedOrders.Add(mapper.Map<CompletedOrder>(exsistingOrder));

                        martiviDbContext.SaveChanges();
                        break;
                    }
            }

            //var CompletedOrder = mapper.Map<CompletedOrder>(exsistingOrder);
            //martiviDbContext.CompletedOrders.Add(CompletedOrder);

            //foreach (var p in exsistingOrder.OrderedProducts)
            //{
            //    var res = martiviDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
            //    if (res != null)
            //    {
            //        res.QuantityInSupply += p.Quantity;
            //    }
            //}

            //martiviDbContext.SaveChanges();
            hubContext.Clients.All.SendAsync("UpdateListing");
            hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
            hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrder", exsistingOrder);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        public IActionResult Post(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            foreach (var p in order.OrderedProducts)
            {
                var res = martiviDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
                if (res != null && res.QuantityInSupply >= p.Quantity)
                {
                    res.QuantityInSupply -= p.Quantity;
                }
                else
                {
                    return BadRequest("არასაკმარისი პროდუქტი");
                }
            }
            var user = martiviDbContext.Users.FirstOrDefault(u => u.UserId == order.User.UserId);
            order.User = user;
            martiviDbContext.Orders.Add(order);
            martiviDbContext.SaveChanges();
            var admins = martiviDbContext.Users.Where(user => user.Type == UserType.Admin);
            foreach (var admin in admins)
            {
                hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrderListing");
            }

            hubContext.Clients.All.SendAsync("UpdateListing");
            hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrderListing");
            hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrderListing");
            return StatusCode(StatusCodes.Status201Created);


        }

        [Route("GetOrders/")]
        [HttpPost]
        public IActionResult GetOrders(User user)
        {
            var orders = martiviDbContext.Orders.Include("OrderedProducts").Include("User").Where(o => o.User.UserId == user.UserId);
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }
        [Route("GetAllOrders/")]
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            int userid;
            if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
            var user = martiviDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");

            var orders = martiviDbContext.Orders.Include("OrderedProducts").Include("User");
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }
    }
}
