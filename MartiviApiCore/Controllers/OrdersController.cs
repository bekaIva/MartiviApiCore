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
using Google.Cloud.Firestore;
using MaleApi.Data;
using MaleApi.Models;
using MaleApi.Models.Users;
using MaleApiCore.Chathub;
using MaleApiCore.FirestoreDataAccess;
using MaleApiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Syncfusion.HtmlConverter;

namespace MaleApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]

    public class OrdersController : ControllerBase
    {
        FirestoreDataAccessLayer firestoreDataAccessLayer = new FirestoreDataAccessLayer();
        UnipayMerchant merchant = new UnipayMerchant();
        private readonly IWebHostEnvironment _webhostingEnvironment;
        MaleDbContext maleDbContext;
        IMapper mapper;
        IHubContext<ChatHub> hubContext;
        public OrdersController(MaleDbContext db, IMapper mapper, IHubContext<ChatHub> hub, IWebHostEnvironment hostingEnvironment)
        {
            _webhostingEnvironment = hostingEnvironment;
            maleDbContext = db;
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
                var exsistingOrder = maleDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);
                exsistingOrder.User = null;

                if(exsistingOrder.Status!= OrderStatus.Completed)
                {
                    foreach (var p in exsistingOrder.OrderedProducts)
                    {
                        var res = maleDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
                        if (res != null)
                        {
                            res.QuantityInSupply += p.Quantity;
                        }
                    }
                }

                foreach (var p in exsistingOrder.OrderedProducts)
                {
                    maleDbContext.OrderedProducts.Remove(p);
                }
                exsistingOrder.OrderedProducts.Clear();
                maleDbContext.SaveChanges();



                maleDbContext.Orders.Remove(exsistingOrder);
                maleDbContext.SaveChanges();

                var admins = maleDbContext.Users.AsQueryable().Where(user => user.Type == UserType.Admin);
                foreach (var admin in admins)
                {
                    hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrderListing");
                }

                hubContext.Clients.All.SendAsync("UpdateListing");
                if (order.User != null) hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrderListing");
                hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrderListing");
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ee)
            {
                return StatusCode(StatusCodes.Status201Created, ee.Message);

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
            var exsistingOrder = maleDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);

            exsistingOrder.Status = OrderStatus.Canceled;
            //var canceledorder = mapper.Map<CanceledOrder>(exsistingOrder);
            //maleDbContext.CanceledOrders.Add(canceledorder);

            //foreach (var p in exsistingOrder.OrderedProducts)
            //{
            //    var res = maleDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
            //    if (res != null)
            //    {
            //        res.QuantityInSupply += p.Quantity;
            //    }
            //}

            maleDbContext.SaveChanges();
            var admins = maleDbContext.Users.AsQueryable().Where(user => user.Type == UserType.Admin);
            foreach (var admin in admins)
            {
                hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
            }

            hubContext.Clients.All.SendAsync("UpdateListing");
            if(order.User!=null) hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
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
            var exsistingOrder = maleDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);
            if (exsistingOrder.Status == order.Status) return Ok();
            exsistingOrder.Status = order.Status;


            maleDbContext.SaveChanges();
            hubContext.Clients.All.SendAsync("UpdateOrderListing");
            if(order.User!=null) hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrder", exsistingOrder);
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
            if (order.OrderedProducts.Count == 0) return BadRequest("ცარიელი შეკვეთა, შეკვეთა არ შეიცავს არცერთ პროდუქტს");
            foreach (var p in order.OrderedProducts)
            {
                var res = maleDbContext.Products.FirstOrDefault(pp => pp.ProductId == p.ProductId);
                if (res != null && res.QuantityInSupply >= p.Quantity)
                {
                    res.QuantityInSupply -= p.Quantity;
                }
                else
                {
                    return BadRequest("არასაკმარისი პროდუქტი");
                }
            }
            if (order.User != null)
            {
                var user = maleDbContext.Users.FirstOrDefault(u => u.UserId == order.User.UserId);
                order.User = user;
            }
            maleDbContext.Orders.Add(order);
            maleDbContext.SaveChanges();




            var admins = maleDbContext.Users.AsQueryable().Where(user => user.Type == UserType.Admin);
            foreach (var admin in admins)
            {
                hubContext.Clients.User(admin.UserId.ToString()).SendAsync("NewOrderMade");
                hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrderListing");
            }

            hubContext.Clients.All.SendAsync("UpdateListing");
            if(order.User!=null) hubContext.Clients.User(order.User.UserId.ToString()).SendAsync("UpdateOrderListing");
            hubContext.Clients.User(User.Identity.Name).SendAsync("UpdateOrderListing");



            return StatusCode(StatusCodes.Status201Created, order);


        }

        [HttpPost]
        [Route("Checkout/")]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                var exsistingOrder = maleDbContext.Orders.Include(ord => ord.OrderedProducts).Include(prd => prd.User).ThenInclude(usr => usr.UserAddresses).ThenInclude(uadr => uadr.Coordinates).Include(ord => ord.OrderAddress).ThenInclude(oad => oad.Coordinates).FirstOrDefault(o => o.OrderId == order.OrderId);
                if (exsistingOrder == null) return BadRequest("Order doesn't exists");
                if (exsistingOrder.Status == OrderStatus.Completed) throw new Exception("შესრულებული შეკვეთა არ საჭირეობს გადახდას.");
                if (exsistingOrder.Status == OrderStatus.Canceled) throw new Exception("გაუქმებული შეკვეთა არ საჭირეობს გადახდას.");
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
                                maleDbContext.SaveChanges();
                                hubContext.Clients.User(exsistingOrder.User.UserId.ToString()).SendAsync("UpdateOrderListing");
                                try
                                {
                                    var adminUsers = maleDbContext.Users.AsQueryable().Where(new Func<User, bool>((user) => { return user.Type == UserType.Admin; }));
                                    foreach (var admin in adminUsers)
                                    {
                                        hubContext.Clients.User(admin.UserId.ToString()).SendAsync("UpdateOrderListing");
                                    }
                                }
                                catch { }

                            }

                        }
                    }
                }
                catch
                {

                }


                maleDbContext.SaveChanges();
                return Ok(res);
            }
            catch (Exception ee)
            {
                return BadRequest("Checkout failed with an error: " + ee.Message);
            }
        }

        [HttpPost]
        [Route("CheckoutFlutter/")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckoutFlutter(FlutterOrder order)
        {
            try
            {
                var qres = await firestoreDataAccessLayer.fireStoreDb.Collection("orders").Document(order.documentId).GetSnapshotAsync();
                if (!qres.Exists)
                {
                    return BadRequest("Order doesn't exists");
                }
                var res = await merchant.ChekoutFlutter(order);
                await firestoreDataAccessLayer.fireStoreDb.Collection("orders").Document(order.documentId).SetAsync(new { Hash = order.Hash, TransactionID = order.TransactionID }, SetOptions.MergeAll);
                if (res == null) throw new Exception("Unknown error returned from merchant.");
                try
                {
                    if (res.Errorcode == 0)
                    {
                        var checkRes = await merchant.CheckStatusFlutter(order);

                        if (res.Errorcode == 0)
                        {
                            PaymentStatus status;
                            if (Enum.TryParse<PaymentStatus>(checkRes.Data.Status, out status))
                            {
                                order.Payment = status;
                                await firestoreDataAccessLayer.fireStoreDb.Collection("orders").Document(order.documentId).SetAsync(new { paymentStatus = order.Payment.ToString() }, SetOptions.MergeAll);
                            }

                        }
                    }
                }
                catch
                {
                }
                return Ok(res);
            }
            catch (Exception ee)
            {
                return BadRequest("Checkout failed with an error: " + ee.Message);
            }
        }





        [Route("GetOrders/")]
        [HttpPost]
        public IActionResult GetOrders(User user)
        {
            var orders = maleDbContext.Orders.Include(ord => ord.OrderedProducts).Include(ord => ord.OrderAddress).ThenInclude(ordadr => ordadr.Coordinates).Include(ord => ord.User).ThenInclude(usr => usr.UserAddresses).ThenInclude(uadr => uadr.Coordinates).Where(o => o.User.UserId == user.UserId);
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [Route("GetOrders/")]
        [HttpGet]
        public IActionResult GetOrders(string userUId)
        {
            var orders = maleDbContext.Orders.Include(ord => ord.OrderedProducts).Include(ord => ord.OrderAddress).ThenInclude(ordadr => ordadr.Coordinates).Where(o => o.UserUId == userUId);
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
            var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
            if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");

            var orders = maleDbContext.Orders.Include(ord => ord.OrderedProducts).Include(ord => ord.OrderAddress).ThenInclude(orda => orda.Coordinates).Include(ord => ord.User).ThenInclude(usr => usr.UserAddresses).ThenInclude(usradr => usradr.Coordinates);
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
                var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
                if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");
                var exsistingOrder = maleDbContext.Orders.Include("OrderedProducts").FirstOrDefault(o => o.OrderId == order.OrderId);




                var pdfRes = GenerateInvoicePDF(exsistingOrder);





                return File(pdfRes, "application/pdf", "diploma.pdf");
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }


        }

        [HttpPost]
        [Route("SetSeen/")]
        public IActionResult PostSetSeen(Order order)
        {
            try
            {
                int userid;
                if (!int.TryParse(User.Identity.Name, out userid)) return BadRequest();
                var user = maleDbContext.Users.FirstOrDefault(user => user.UserId == userid);
                if (user.Type != UserType.Admin) return BadRequest("არა ადმინისტრატორი მომხმარებელი");
                var exsistingOrder = maleDbContext.Orders.FirstOrDefault(o => o.OrderId == order.OrderId);

                if (!exsistingOrder.IsSeen)
                {
                    exsistingOrder.IsSeen = true;
                    maleDbContext.SaveChanges();
                    hubContext.Clients.User(user.UserId.ToString()).SendAsync("UpdateOrderListing");
                }

                return Ok();
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
            dt.Columns.AddRange(new DataColumn[7] {
                            new DataColumn("ProductId", typeof(string)),
                            new DataColumn("პროდიქტი", typeof(string)),
                            new DataColumn("აღწერა", typeof(string)),
                            new DataColumn("ფასი", typeof(string)),
                            new DataColumn("წონა", typeof(string)),
                            new DataColumn("რაოდენობა", typeof(int)),
                            new DataColumn("სრულად", typeof(string))});
            foreach (var p in order.OrderedProducts)
            {
                dt.Rows.Add(p.ProductId, p.Name, p.Description, p.Price.ToString("0.######"), p.Weight, p.Quantity, (p.Quantity * p.Price).ToString("0.######"));
            }
            if (order.DeliveryFee > 0)
            {
                dt.Rows.Add("", "მიწოდება", "მიწოდების საფასური", order.DeliveryFee.ToString("0.######"), "", 1, order.DeliveryFee.ToString("0.######"));
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
                    sb.Append(dtt.ToString());
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
                    sb.Append((order.OrderedProducts.Sum(p => p.Quantity * p.Price) + order.DeliveryFee).ToString("0.######") + "₾");
                    sb.Append("</td>");
                    sb.Append("</tr></table>");
                    sb.Append("<table width='100%'><tr><td align='right'><b>გადახდა: </b>" + order.Payment.ToString() + "</td></tr></table>");

                    HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                    WebKitConverterSettings settings = new WebKitConverterSettings();
                    settings.WebKitPath = Path.Combine(_webhostingEnvironment.ContentRootPath, "QtBinariesWindows");
                    htmlConverter.ConverterSettings = settings;
                    Syncfusion.Pdf.PdfDocument document = htmlConverter.Convert(sb.ToString(), "");
                    MemoryStream ms = new MemoryStream();
                    document.Save(ms);
                    ms.Position = 0;
                    return ms;

                }
            }
        }
    }
}
