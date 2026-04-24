using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;
using WinPinAgent.Infrastructure.Data.Configurations;

namespace WinPinAgent.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<PartRequest> PartRequests => Set<PartRequest>();
        public DbSet<Offer> Offers => Set<Offer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PartRequestConfiguration());
            modelBuilder.ApplyConfiguration(new OfferConfiguration());
        }
    }
}
