using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MartiviApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Data
{
    public class MartiviDbContext : DbContext
    {
        public MartiviDbContext(DbContextOptions<MartiviDbContext> options):base(options)
        {

        }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}