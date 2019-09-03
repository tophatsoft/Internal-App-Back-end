using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    { private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationSettings _appSettings;
        public IConfiguration Configuration { get; }

        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._appSettings = appSettings.Value;
            this.Configuration = configuration;
        }

        // ADMIN
        [HttpPost]
        [Authorize(Roles ="Admin")]
        [Route("Register")]
        public async Task<Object> Register(RegisterModel model)
        {
            model.Role = "User";

            var applicationUser = new ApplicationUser()
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                Email = model.Email,
                LastName = model.LastName,
                RegisteredDate = DateTime.Today,
                Department = model.Department,
                Position = model.Position,
                PassChanged = false
            };
            try
            {
                var randomPass = GenerateRandomPassword();
                var result = await _userManager.CreateAsync(applicationUser, randomPass);
                await _userManager.AddToRoleAsync(applicationUser, model.Role);

                await SendEmailAsync(applicationUser.FirstName,applicationUser.Email, applicationUser.UserName, randomPass);
                
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("userlist")]
        public async Task<List<UserModel>> ListUsers()
        {
            var users =await _userManager.GetUsersInRoleAsync("user");
            List<UserModel> userList = new List<UserModel>();
            foreach(var user in users)
            {
               userList.Add(new UserModel {Department = user.Department, Email = user.Email, FirstName =user.FirstName, LastName = user.LastName, Position = user.Position,UserName = user.UserName, RegisteredDate = user.RegisteredDate });
            }
            return userList ;
           
        }

        //   USERS
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if(user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                //get the role assigned to the user
                var role = await _userManager.GetRolesAsync(user);
                IdentityOptions _options = new IdentityOptions();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                        new Claim("UserID", user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, role.FirstOrDefault())
                        
                    }),
                    Expires =DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }
        }
        //random pass function
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
        "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
        "abcdefghijkmnopqrstuvwxyz",    // lowercase
        "0123456789",                   // digits
        "!@$?_-"                        // non-alphanumeric
    };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        // SEND EMAIL FUNCTION
        public async Task SendEmailAsync(string name, string email, string userName, string password)
        {

            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress(Configuration["DomainEmail:Email"]);
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress(name,email);
            message.To.Add(to);

            message.Subject = "Welocome, " + name;

            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain) {Text = "Log in using the following:\n" + "UserName: " + userName +"\n" + "Password: " +password +"\n\n" +"Change password here:\n" + "http://localhost:4200/changepass" };

            using(var user = new SmtpClient())
            {
                user.Connect(Configuration["DomainEmail:Domain"], 465, true);
                user.Authenticate(Configuration["DomainEmail:Email"], Configuration["DomainEmail:Password"]);
                await user.SendAsync(message);
                await user.DisconnectAsync(true);
            }
            
        }
  

    }
}