using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MartiviApi.Models;
using MartiviApiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Data
{
    public class MartiviDbContext : DbContext
    {
        public MartiviDbContext(DbContextOptions<MartiviDbContext> options):base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
    }
}