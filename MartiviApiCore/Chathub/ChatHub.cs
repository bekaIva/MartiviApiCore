using MartiviApi.Data;
using MartiviApi.Models.Users;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;

namespace MartiviApiCore.Chathub
{

    public class ChatHub : Hub
    {
        private IServiceProvider _sp;
        public ChatHub(IServiceProvider sp)
        {
            _sp = sp;
        }
        public async Task JoinChat(string user)
        {

            var c = Context.User.Identity.Name;
            var us = Clients.User(c);
        }

        public async Task LeaveChat(string user)
        {

        }
        void RemoveOldItems<T>(ICollection<T> list)
        {
            try
            {
                //if (list.Count > 30)
                //{
                //    var colList = list.ToList();
                //    List<T> itemsToremove = new List<T>();
                //    for (int i = 0; i < list.Count-30; i++)
                //    {
                //        itemsToremove.Add(colList[i]);
                //    }
                //    foreach (var item in itemsToremove)
                //    {
                //        list.Remove(item);
                //    }
                //}
            }
            catch
            {

            }
        }
        public async Task SendMessage(ChatMessage chatMessage)
        {
            
            using (var scope = _sp.CreateScope())
            {
                var martiviDbContext = scope.ServiceProvider.GetRequiredService<MartiviDbContext>();

                //...
                
                int id;
                int.TryParse(Context.User.Identity.Name, out id);
                if (id > 0 != true) return;
                var senderUser = martiviDbContext.Users.Include("Messages").Single(c => c.UserId == id);
                chatMessage.OwnerProfileImage = senderUser.ProfileImageUrl;
                var chmSerialized = JsonConvert.SerializeObject(chatMessage);
                RemoveOldItems(senderUser.Messages);
                senderUser.Messages.Add(chatMessage);
                switch (chatMessage.Target)
                {
                    case MessageTarget.Admin:
                        {
                            var adminUsers = martiviDbContext.Users.Include("Messages").Where(c => c.Type == MartiviApi.Models.UserType.Admin);
                            foreach (var adminuser in adminUsers)
                            {
                                if (adminuser.UserId == id) continue;
                                var incomingMessage = JsonConvert.DeserializeObject<ChatMessage>(chmSerialized);
                                incomingMessage.TemplateType = TemplateType.IncomingText;
                                RemoveOldItems(adminuser.Messages);
                                adminuser.Messages.Add(incomingMessage);
                                var chatAdminuser = Clients.User(adminuser.UserId.ToString());
                                if (chatAdminuser != null) chatAdminuser.SendAsync("ReceiveMessage", incomingMessage);
                            }
                            break;
                        }
                    case MessageTarget.TargetUser:
                        {
                            var targetUser = martiviDbContext.Users.Include("Messages").FirstOrDefault(c => c.UserId==chatMessage.TargetUser);
                           
                                if (targetUser.UserId == id) return;
                                var incomingMessage = JsonConvert.DeserializeObject<ChatMessage>(chmSerialized);
                                incomingMessage.TemplateType = TemplateType.IncomingText;
                            RemoveOldItems(targetUser.Messages);
                            targetUser.Messages.Add(incomingMessage);
                                var chattargetUser = Clients.User(targetUser.UserId.ToString());
                                if (chattargetUser != null) chattargetUser.SendAsync("ReceiveMessage", incomingMessage);
                            
                            break;
                        }
                    case MessageTarget.Global:
                        {
                            var targetUsers = martiviDbContext.Users.Include("Messages").Where(c => c.UserId != id);

                            foreach (var targetuser in targetUsers)
                            {
                                if (targetuser.UserId == id) continue;
                                var incomingMessage = JsonConvert.DeserializeObject<ChatMessage>(chmSerialized);
                                incomingMessage.TemplateType = TemplateType.IncomingText;
                                RemoveOldItems(targetuser.Messages);
                                targetuser.Messages.Add(incomingMessage);
                                var chattargetuser = Clients.User(targetuser.UserId.ToString());
                                if (chattargetuser != null) chattargetuser.SendAsync("ReceiveMessage", incomingMessage);
                            }
                            break;
                        }
                    case MessageTarget.AllUsersExceptAdmin:
                        {
                            var NonadminUsers = martiviDbContext.Users.Include("Messages").Where(c => c.Type == MartiviApi.Models.UserType.Client);
                            foreach (var nonadminuser in NonadminUsers)
                            {
                                if (nonadminuser.UserId == id) continue;
                                var incomingMessage = JsonConvert.DeserializeObject<ChatMessage>(chmSerialized);
                                incomingMessage.TemplateType = TemplateType.IncomingText;
                                RemoveOldItems(nonadminuser.Messages);
                                nonadminuser.Messages.Add(incomingMessage);
                                var chatnonadminuser = Clients.User(nonadminuser.UserId.ToString());
                                if (chatnonadminuser != null) chatnonadminuser.SendAsync("ReceiveMessage", incomingMessage);
                            }
                            break;
                        }
                }



                //Clients.Caller.SendAsync("ReceiveMessage", user, message);

                //var adminUser = dbcontext.Users.FirstOrDefault(user => user.Type == MartiviApi.Models.UserType.Admin);

                //Clients.User(adminUser.Username).SendAsync(user, message);
                martiviDbContext.Database.OpenConnection();
                try
                {

                    martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] ON");
                    martiviDbContext.SaveChanges();

                }
                finally
                {
                    martiviDbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Users] OFF");
                    martiviDbContext.Database.CloseConnection();
                }
            }

        }
        public async Task SendMessageToPublick(string user, string message)
        {
            Clients.All.SendAsync(user, message);
        }
        public async Task SendMessageToAdmin(string user, string message)
        {

          



            //Clients.Caller.SendAsync("ReceiveMessage", user, message);

            //var adminUser = martiviDbContext.Users.FirstOrDefault(user => user.Type == MartiviApi.Models.UserType.Admin);

            //Clients.User(adminUser.Username).SendAsync(user, message);

        }


    }
}
