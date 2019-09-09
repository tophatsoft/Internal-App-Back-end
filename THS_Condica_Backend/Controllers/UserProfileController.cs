using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
       

        public UserProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
          
        }

        [HttpGet]
        [Authorize]
        [Route("firstLoggin")]
        //GET: /api/UserProfile
        public async Task<Object> FirstLoggin()
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var user = await _userManager.FindByIdAsync(userId);

            return new
            {
                user.PassChanged,
                user.UserName,
                user.Email
            };
        }

        [HttpPut]
        [Authorize]
        //PUT: /api/UserProfile
        public async Task<ApiResponse> UpdateUserProfile([FromBody] UpdateUserProfileModel userProfile)
        {
            var emailChanged = false;

            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var user = await _userManager.FindByIdAsync(userId);
            //user.Notes = userProfile.Notes;
            if (userProfile.FirstName != null)
            {
                user.FirstName = userProfile.FirstName;
            }

            if (userProfile.LastName != null)
            {
                user.LastName = userProfile.LastName;
            }

            if (userProfile.Department != null)
            {
                user.Department = userProfile.Department;
            }

            if (userProfile.Position != null)
            {
                user.Position = userProfile.Position;
            }

            if (userProfile.UserName != null)
            {
                user.UserName = userProfile.UserName;
            }

            if (userProfile.Email != null &&
            !string.Equals(userProfile.Email.Replace(" ", ""), user.NormalizedEmail))
            {
                user.Email = userProfile.Email;

                user.EmailConfirmed = false;

                emailChanged = true;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return new ApiResponse();
            else
                return new ApiResponse
                {
                    ErrorMessage = result.Errors.ToString()
                };         

        }
        [HttpPut]
        [Route("changePass")]
        public async Task<Object> UpdateUserPassword([FromBody] UpdateUserPasswordModel model)
        {
            //var user = await _userManager.GetUserAsync(HttpContext.User); //works 2
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                user.PassChanged = true;
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
               
                return Ok(result);

            }else
            {
                return BadRequest(new { message = "Wrong password" });
            }

        }
    }
}