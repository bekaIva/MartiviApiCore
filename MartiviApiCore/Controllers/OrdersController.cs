using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using AutoMapper;
using MartiviApi.Data;
using MartiviApi.Models;
using MartiviApi.Models.Users;
using MartiviApiCore.Chathub;
using MartiviApiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Syncfusion.HtmlConverter;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]

    public class OrdersController : ControllerBase
    {
        UnipayMerchant merchant = new UnipayMerchant();
        private readonly IWebHostEnvironment _webhostingEnvironment;
        MartiviDbContext martiviDbContext;
        IMapper mapper;
        IHubContext<ChatHub> hubContext;
        public OrdersController(MartiviDbContext db, IMapper mapper, IHubContext<ChatHub> hub, IWebHostEnvironment hostingEnvironment)
        {
            _webhostingEnvironment = hostingEnvironment;
            martiviDbContext = db;
            this.mapper = mapper;
            hubContext = hub;
        }

        [HttpPost]
        [Route("DeleteOrder/")]
        public IActionResult PostDeleteOrder(Order order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);
                exsistingOrder.User = null;


                foreach (var p in exsistingOrder.OrderedProducts)
                {
                    martiviDbContext.OrderedProducts.Remove(p);
                }
                exsistingOrder.OrderedProducts.Clear();
                martiviDbContext.SaveChanges();



                martiviDbContext.Orders.Remove(exsistingOrder);
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
            catch (Exception ee)
            {
                return StatusCode(StatusCodes.Status201Created,ee.Message);

            }


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
        public async Task<IActionResult> Post(Order order)
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

           

            return StatusCode(StatusCodes.Status201Created,order);


        }

        [HttpPost]
        [Route("Checkout/")]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").Include("User").Include("User.UserAddresses").Include("OrderAddress").FirstOrDefault(o => o.OrderId == order.OrderId);
                if (exsistingOrder == null) return BadRequest("Order doesn't exists");              
                var res = await merchant.Chekout(exsistingOrder);                
                if (res == null) throw new Exception("Unknown error returned from merchant.");
                try
                {
                    if (res.Errorcode == 0)
                    {
                        var checkRes = await merchant.CheckStatus(exsistingOrder);                        
                       
                        if (res.Errorcode == 0)
                        {
                            PaymentStatus status;
                            if (Enum.TryParse<PaymentStatus>(checkRes.Data.Status, out status))
                            {
                                exsistingOrder.Payment = status;
                                martiviDbContext.SaveChanges();
                                hubContext.Clients.User(exsistingOrder.User.UserId.ToString()).SendAsync("UpdateOrderListing");
                                
                            }

                        }
                    }
                }
                catch
                {

                }


                martiviDbContext.SaveChanges();
                return Ok(res);
            }
            catch (Exception ee)
            {
                return BadRequest("Checkout failed with error: " + ee.Message);
            }
        }



        [Route("GetOrders/")]
        [HttpPost]
        public IActionResult GetOrders(User user)
        {
            var orders = martiviDbContext.Orders.Include("OrderedProducts").Include("OrderAddress").Include("User").Include("User.UserAddresses").Where(o => o.User.UserId == user.UserId);
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

            var orders = martiviDbContext.Orders.Include("OrderedProducts").Include("OrderAddress").Include("User").Include("User.UserAddresses");
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [HttpPost]
        [Route("GetInvoice/")]
        public IActionResult PostGetInvoice(Order order)
        {
            try
            {
                int userid;
                if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
                var user = martiviDbContext.Users.FirstOrDefault(user => user.UserId == userid);
                if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");
                var exsistingOrder = martiviDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);


              
              
                var pdfRes = GenerateInvoicePDF(exsistingOrder);

               



                return File(pdfRes, "application/pdf", "diploma.pdf");
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }


        }
        protected Stream GenerateInvoicePDF(Order order)
        {
            //Dummy data for Invoice (Bill).
            string companyName = "მარტივი";
            int orderNo = order.OrderId;
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[6] {
                            new DataColumn("ProductId", typeof(string)),
                            new DataColumn("პროდიქტი", typeof(string)),
                            new DataColumn("აღწერა", typeof(string)),
                            new DataColumn("ფასი", typeof(double)),
                            new DataColumn("რაოდენობა", typeof(int)),
                            new DataColumn("სრულად", typeof(double))});
            foreach (var p in order.OrderedProducts)
            {
                dt.Rows.Add(p.ProductId, p.Name,p.Description, p.Price, p.Quantity, Math.Round(p.Quantity*p.Price,1));               
            }
           

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    StringBuilder sb = new StringBuilder();

                    //Generate Invoice (Bill) Header.
                    sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
                    sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>შეკვეთის ფურცელი</b></td></tr>");
                    sb.Append("<tr><td colspan = '2'></td></tr>");
                    sb.Append("<tr><td><b>შეკვეთის ნომერი: </b>");
                    sb.Append(orderNo);
                    sb.Append("</td><td align = 'right'><b>თარიღი: </b>");
                    DateTime dtt = new DateTime(order.OrderTimeTicks);
                    sb.Append( dtt.ToString());
                    sb.Append(" </td></tr>");
                    sb.Append("<tr><td colspan = '2'><b>კომპანიის სახელი: </b>");
                    sb.Append(companyName);
                    sb.Append("</td></tr>");
                    sb.Append("</table>");
                    sb.Append("<br />");

                    //Generate Invoice (Bill) Items Grid.
                    sb.Append("<table border = '1' width='100%'>");
                    sb.Append("<tr>");
                    foreach (DataColumn column in dt.Columns)
                    {
                        sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                        sb.Append(column.ColumnName);
                        sb.Append("</th>");
                    }
                    sb.Append("</tr>");
                    foreach (DataRow row in dt.Rows)
                    {
                        sb.Append("<tr>");
                        foreach (DataColumn column in dt.Columns)
                        {
                            sb.Append("<td>");
                            sb.Append(row[column]);
                            sb.Append("</td>");
                        }
                        sb.Append("</tr>");
                    }
                    sb.Append("<tr><td align = 'right' colspan = '");
                    sb.Append(dt.Columns.Count - 1);
                    sb.Append("'>სრულად</td>");
                    sb.Append("<td>");
                    sb.Append(dt.Compute("sum(სრულად)", "")+ "₾");
                    sb.Append("</td>");
                    sb.Append("</tr></table>");
                    sb.Append("<table width='100%'><tr><td align='right'><b>გადახდა: </b>"+order.Payment.ToString()+"</td></tr></table>");
                    
                    HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                    WebKitConverterSettings settings = new WebKitConverterSettings();
                    settings.WebKitPath = Path.Combine(_webhostingEnvironment.ContentRootPath, "QtBinariesWindows");
                    htmlConverter.ConverterSettings = settings;
                    Syncfusion.Pdf.PdfDocument document = htmlConverter.Convert(sb.ToString(),"");
                    MemoryStream ms = new MemoryStream();
                    document.Save(ms);
                    ms.Position = 0;
                    return ms;
                    
                }
            }
        }
    }
}
