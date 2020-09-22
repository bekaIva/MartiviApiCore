using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MartiviApi.Data;
using MartiviApiCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using MartiviApiCore.Chathub;
using Microsoft.AspNetCore.Authorization;

namespace MartiviApiCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        MartiviDbContext context;
        IHubContext<ChatHub> hubContext;
        public InfoController(MartiviDbContext dbContext, IHubContext<ChatHub> hub)
        {
            hubContext = hub;
            context = dbContext;
        }

        [Authorize]
        [HttpPost("AddInfo")]
        public IActionResult AddInfo(Info info)
        {
            Info infoSet = context.Infos.FirstOrDefault();
            if (infoSet == null)
            {
                context.Infos.Add(info);
            }
            else
            {
                infoSet.Description = info.Description;
                infoSet.Name = info.Name;
                infoSet.Version = info.Version;
            }
            context.SaveChanges();
            hubContext.Clients.All.SendAsync("UpdateInfo");
            return Ok();
        }
        [HttpGet("GetInfo")]
        public IActionResult GetInfo()
        {
            Info info = context.Infos.FirstOrDefault();

            return Ok(info);
        }
        
        [HttpGet("GetSettings")]
        public IActionResult GetSettings()
        {
            return Ok(context.Settings);
        }

        [HttpPost("SetSettings")]
        [Authorize]
        public IActionResult SetSetting(Setting[] settings)
        {
            try
            {
                foreach (var setting in settings) 
                {
                    try
                    {
                        var set = context.Settings.FirstOrDefault(s => s.Name == setting.Name);
                        if (set != null)
                        {
                            set.Value = setting.Value;
                        }
                        else
                        {
                            context.Settings.Add(setting);
                        }
                        
                    }
                    catch
                    {

                    }
                    
                }
                context.SaveChanges();
                hubContext.Clients.All.SendAsync("UpdateSettings");
                return Ok();
            }
            catch(Exception ee)
            {
                return BadRequest(ee.Message);
            }
        }
    }
}