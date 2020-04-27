using MartiviApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MartiviApiCore.Services
{
    public class ChekoutData
    {
        public string Status { get; set; }
        public string MerchantOrderID { get; set; }
    }

    public class CheckoutResult
    {
        public int Errorcode { get; set; }
        public string Message { get; set; }
        public ChekoutData Data { get; set; }
    }

    public class CheckoutData
    {
        public string Checkout { get; set; }
        public string UnipayOrderHashID { get; set; }
    }

    public class CreateOrderResult
    {
        public int Errorcode { get; set; }
        public string Message { get; set; }
        public CheckoutData Data { get; set; }
    }
    public class Transaction
    {
        public string MerchantID { get; set; }
        public string Hash { get; set; }
        public string TransactionID { get; set; }
    }
    public class TransactionData
    {
        public string Status { get; set; }
        public string UnipayOrderHashID { get; set; }
    }

    public class CheckTransactionResult
    {
        public int Errorcode { get; set; }
        public string Message { get; set; }
        public TransactionData Data { get; set; }
    }

    public class CreateOrderModel
    {

        public string MerchantID { get; set; }
        public string MerchantUser { get; set; }
        public string MerchantOrderID { get; set; }
        public string OrderPrice { get; set; }
        public string OrderCurrency { get; set; }
        public string BackLink { get; set; }
        public string Mlogo { get; set; }
        public string Mslogan { get; set; }
        public string Language { get; set; }
        public string OrderDescription { get; set; }
        public string OrderName { get; set; }
        public string Hash { get; set; }
        public List<string> Items { get; set; }
    }

    public class UnipayMerchant
    {
        public string MerchantID { get; set; } = "7603109";
        public string SecretKey { get; set; } = "85DAED23-CA16-4CA9-8673-1E1C2E506526";
        HttpClient client = new HttpClient();

        //byte[] getLogo()
        //{
        //    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        //    var ress = assembly.GetManifestResourceNames();
        //    using (var stream = assembly.GetManifestResourceStream("MartiviApiCore.Resources.ic_launcher.png"))
        //    {
        //        byte[] buffer = new byte[stream.Length];
        //        stream.Read(buffer, 0, buffer.Length);
        //        return buffer;
        //        // TODO: use the buffer that was read
        //    }
        //}
       public CreateOrderModel GenerateOrderCreateRequest(Order order)
        {
            CreateOrderModel om = new CreateOrderModel();
            om.MerchantID = MerchantID;
            om.MerchantUser = order.User.Username;
            om.MerchantOrderID = order.OrderId.ToString();
            om.OrderPrice = (100 * order.OrderedProducts.Sum((p) => { return p.Quantity * p.Price; })).ToString();
            om.OrderCurrency = "GEL";
            om.BackLink = Convert.ToBase64String(Encoding.ASCII.GetBytes("http://martivi.net/CheckoutResult?MerchantOrderID=" + order.OrderId.ToString()));
            om.Mlogo = Convert.ToBase64String(Encoding.ASCII.GetBytes("http://martivi.net/images/ic_launcher.png"));
            om.Mslogan = "შეუკვეთე მარტივად";
            om.Language = "GE";




                om.Items = new List<string>(order.OrderedProducts.Select((p) => { return (p.Price * 100) + "|" + p.Quantity.ToString() + "|" + p.Name + "|" + p.Description; }));
           
            string PasswordStr = GetPassword(om);


            om.Hash = PasswordStr;
            return om;
        }
        string GetPassword(object myObject)
        {
            StringBuilder paswordStrb = new StringBuilder();
            paswordStrb.Append(SecretKey);
            Type myType = myObject.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                if (prop.Name == "Hash") continue;
                object propValue = prop.GetValue(myObject, null);
                if (propValue != null && propValue is string s)
                {
                    paswordStrb.Append("|").Append(s);
                }

                // Do something with propValue
            }

            //string PasswordStr = "85DAED23-CA16-4CA9-8673-1E1C2E506526|" + om.MerchantID + "|" + om.MerchantUser + "|" +
            //    om.MerchantOrderID + "|" + om.OrderPrice + "|" + om.OrderCurrency + "|" + om.BackLink + "|" + om.Mlogo + "|" + om.Mslogan + "|"
            //    + om.Language;
            var str = paswordStrb.ToString();
            return CalculateMD5Hash(paswordStrb.ToString());
        }
        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2").ToLower());
            }
            return sb.ToString().ToLower();
        }
        public async Task<CheckTransactionResult> CheckStatus(Order o)
        {
            if(!string.IsNullOrEmpty(o?.TransactionID))
            {
                Transaction t = new Transaction() {TransactionID=o.TransactionID, MerchantID=MerchantID };
                string checkTransactionUrl = "https://api.unipay.com/checkout/transaction";

                var hash = GetPassword(t);
                t.Hash = hash;
                var json = JsonConvert.SerializeObject(t, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var encodedStr = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", MerchantID, hash)));
                var authorizationKey = "Basic " + encodedStr;    // Note: Basic case sensitive
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, checkTransactionUrl);
                requestMessage.Content = content;
                requestMessage.Headers.Add("Authorization", authorizationKey);
                var response = await client.SendAsync(requestMessage);
                string resStr = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var res = JsonConvert.DeserializeObject<CheckTransactionResult>(resStr);
                        if(res!=null)
                        {
                            return res;
                        }
                        else
                        {
                            throw new Exception("No checkout response");
                        }
                        
                    }
                    catch (Exception ee)
                    {
                        throw new Exception("No response. "+ee.Message);
                    }
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception(resStr);
                }
                throw new Exception("Get addresses failed! \nError Code: " + response.StatusCode + "\n" + resStr);

            }
            else
            {
                throw new Exception("No transaction created for this order.");
            }
        }
        public async Task<CreateOrderResult> Chekout(Order order)
        {
            string createOrderAddress = "https://api.unipay.com/checkout/createorder";
            var ordercreate = GenerateOrderCreateRequest(order);
            order.Hash = ordercreate.Hash;
            var json = JsonConvert.SerializeObject(ordercreate, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var encodedStr = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", ordercreate.MerchantID, ordercreate.Hash)));
            var authorizationKey = "Basic " + encodedStr;    // Note: Basic case sensitive
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, createOrderAddress);
            requestMessage.Content = content;
            requestMessage.Headers.Add("Authorization", authorizationKey);
            var response = await client.SendAsync(requestMessage);
            //var response = await sClient.GetResponsePost(createOrderAddress, content, useCloudFlareBypass: false);

            string resStr = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var res = JsonConvert.DeserializeObject<CreateOrderResult>(resStr);
                    order.TransactionID = res?.Data?.UnipayOrderHashID;
                    return res;
                }
                catch (Exception ee)
                {
                    throw new Exception("გადახდის ინიცირება ვერ მოხერხდა.");
                }
            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new Exception(resStr);
            }
            throw new Exception("Get addresses failed! \nError Code: " + response.StatusCode + "\n" + resStr);
        }
    }
}
