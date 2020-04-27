using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MartiviApi.Data;
using MartiviApi.Models;
using MartiviApiCore.Chathub;
using MartiviApiCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MartiviApiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckoutResultController : Controller
    {
        UnipayMerchant merchant = new UnipayMerchant();
        MartiviDbContext martiviDbContext;
        IHubContext<ChatHub> hubContext;
        public CheckoutResultController(MartiviDbContext db, IHubContext<ChatHub> hub)
        {
            hubContext = hub;
            martiviDbContext = db;
        }
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(string MerchantOrderID)
        {
            try
            {
                var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").Include("User").FirstOrDefault(o => o.OrderId.ToString() == MerchantOrderID);
                if (exsistingOrder != null)
                {
                    var oc = merchant.GenerateOrderCreateRequest(exsistingOrder);
                    if (string.IsNullOrEmpty(exsistingOrder.Hash)) throw new Exception("შეკვეთაზე არ არის გადახდა ინიცირებული.");
                    if (!(oc.Hash?.Equals(exsistingOrder.Hash) ?? false)) { throw new Exception("Invalid order!"); }
                    var res = await merchant.CheckStatus(exsistingOrder);
                    if (res.Errorcode == 0)
                    {
                        PaymentStatus status;
                        if (Enum.TryParse<PaymentStatus>(res.Data.Status, out status))
                        {
                            exsistingOrder.Payment = status;
                            martiviDbContext.SaveChanges();
                            if (User.Identity.Name != exsistingOrder.User.UserId.ToString())
                            {
                                hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrderListing");
                            }
                            hubContext.Clients.User(exsistingOrder.User.UserId.ToString()).SendAsync("UpdateOrderListing");

                            try
                            {
                                var adminUsers = martiviDbContext.Users.Where(new Func<User, bool>((user) => { return user.Type == UserType.Admin; }));
                                foreach (var admin in adminUsers)
                                {
                                    if (admin.UserId != exsistingOrder.User.UserId)
                                    {
                                        hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrderListing");
                                    }
                                }
                            }
                            catch { }
                            return Ok(status.ToString());
                        }
                        else
                        {
                            return Ok(res.Data.Status);
                        }
                    }
                    else
                    {
                        throw new Exception("Error code: " + res.Errorcode + "\nMessage" + res.Message);
                    }
                }
                else
                {
                    throw new Exception("შეკვეთა არ მოიძებნა." + MerchantOrderID);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
