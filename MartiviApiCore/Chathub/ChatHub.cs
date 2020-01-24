using MartiviApi.Data;
using MartiviApi.Models.Users;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApiCore.Chathub
{
   
    public class ChatHub : Hub
    {
        MartiviDbContext dbcontext;
        private IServiceProvider _sp;
        public ChatHub(IServiceProvider sp)
        {
            _sp = sp;
            using (var scope = _sp.CreateScope())
            {
                dbcontext = scope.ServiceProvider.GetRequiredService<MartiviDbContext>();
                //...
            }
           

        }
        public async Task JoinChat(string user)
        {
            
            var c = Context.User.Identity.Name;
            var us = Clients.User(c);
            await us.SendAsync("ReceiveMessage", user, "gamarjoba");
        }

        public async Task LeaveChat(string user)
        {
            
        }

        public async Task SendMessageToPublick(string user, string message)
        {
            Clients.All.SendAsync(user, message);
        }
        public async Task SendMessageToAdmin(string user, string message)
        {
            Clients.Caller.SendAsync("ReceiveMessage",user, message);
            ChatMessage userMessage = new ChatMessage();
           
            var adminUser = dbcontext.Users.FirstOrDefault(user => user.Type == MartiviApi.Models.UserType.Admin);
            
            Clients.User(adminUser.Username).SendAsync(user,message);
            
        }


    }
}
