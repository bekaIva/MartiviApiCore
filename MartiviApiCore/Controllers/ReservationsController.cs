using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MartiviApi.Data;
using MartiviApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MartiviApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
      
        MartiviDbContext martiviDbContext;

        public ReservationsController(MartiviDbContext db)
        {
            martiviDbContext = db;
        }

        [HttpPost]
        public IActionResult Post(Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            martiviDbContext.Reservations.Add(reservation);
            martiviDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
