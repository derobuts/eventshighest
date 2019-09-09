using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Data
{
    public class AppUser : IdentityUser<int>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
    }
    public class ApplicationRole : IdentityRole<int>
    {
    }
    public class ApplicationDbContext : IdentityDbContext<AppUser, ApplicationRole, int>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region "Seed Data"

            #endregion
        }
    }
}
