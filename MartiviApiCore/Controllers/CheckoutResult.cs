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
        public async Task<IActionResult> Get(string hash,string UnipayOrderID,string MerchantOrderID, string Status, string Reason)
        {
            try
            {
                var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").Include("User").FirstOrDefault(o => o.OrderId.ToString() == UnipayOrderID);
                if (exsistingOrder != null)
                {
                    var res = await merchant.CheckStatus(exsistingOrder);
                    if(res.Errorcode==0)
                    {
                        PaymentStatus status;
                        if (Enum.TryParse<PaymentStatus>(res.Data.Status, out status))
                        {
                            exsistingOrder.Payment = status;
                            martiviDbContext.SaveChanges();
                            hubContext.Clients.User(exsistingOrder.User.UserId.ToString()).SendAsync("UpdateOrderListing");
                            return Ok(status);
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
                    throw new Exception("შეკვეთა არ მოიძებნა.");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
