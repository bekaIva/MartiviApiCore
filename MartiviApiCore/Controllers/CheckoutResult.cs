using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MartiviApiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckoutResultController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string hash,string UnipayOrderID,string MerchantOrderID, string Status, string Reason)
        {
            return Ok();
        }
        public IActionResult Get()
        {
            return Ok();
        }


    }
}
