using ChefApi.DAL.Data;
using ChefApi.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefApi.DAL.Utils
{
    public class SeedData : ISeedData
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public SeedData(UserManager<ApplicationUser>userManager,RoleManager<IdentityRole>roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task DataSeeding()
        {

            if (!(await _context.Database.GetAppliedMigrationsAsync()).Any() )
            {
                await _context.Database.MigrateAsync();
            }
        }

        public async Task IdentityDataSeeding()
        {
           if(!await _roleManager.Roles.AnyAsync())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
                await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
            }
            if (!await _userManager.Users.AnyAsync())
            {
                var user1 = new ApplicationUser()
                {
                    Email = "boshrasami@gmail.com",
                    UserName = "Boshrasami",
                    EmailConfirmed = true
                };
                var user2 = new ApplicationUser()
                {
                    Email = "salma12@gmail.com",
                    UserName = "Salma123",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(user1, "Pass@1234");
                await _userManager.CreateAsync(user2, "Pass@1234");
                await _userManager.AddToRoleAsync(user1, "Admin");
                await _userManager.AddToRoleAsync(user2, "User");
            }
           await _context.SaveChangesAsync();
            
        }
    }
}
