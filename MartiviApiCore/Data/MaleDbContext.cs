using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaleApi.Models;
using MaleApi.Models.Users;
using MaleApiCore.Models;
using MaleApiCore.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace MaleApi.Data
{
    public class MaleDbContext : DbContext
    {
        public MaleDbContext(DbContextOptions<MaleDbContext> options) : base(options)
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
        public DbSet<MaleApiCore.Models.HomeViewModel> HomeViewModel { get; set; }
    }
}