using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend
{
    public class UserDbContext: IdentityDbContext
    {
        public UserDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<CheckInModel> CheckIn { get; set; }

        

    }
}
