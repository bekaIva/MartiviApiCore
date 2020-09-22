using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MartiviApi.Models;
using MartiviApi.Models.Users;
using MartiviApiCore.Models;
using MartiviApiCore.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace MartiviApi.Data
{
    public class MartiviDbContext : DbContext
    {
        public MartiviDbContext(DbContextOptions<MartiviDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        //public DbSet<CanceledOrder> CanceledOrders { get; set; }
        //public DbSet<CompletedOrder> CompletedOrders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OrderedProduct> OrderedProducts { get; set; }
        public DbSet<Info> Infos { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<OrderAddress> OrderAddresses { get; set; }
        public DbSet<PasswordChangeStore> PasswordChangeStores { get; set; }
        public DbSet<MartiviApiCore.Models.HomeViewModel> HomeViewModel { get; set; }
    }
}