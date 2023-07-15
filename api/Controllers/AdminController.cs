using IdaWebApplicationTemplate.Data;
using IdaWebApplicationTemplate.Services;
using IdaWebApplicationTemplate.Settings;
using IdaWebApplicationTemplate.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdaWebApplicationTemplate.Controllers
{
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly bool _useTwoFactorAuth;
        private readonly ITwoFactorAuthenticationService _twoFactorAuth;

        public AdminController(
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            IOptions<SecuritySettings> options,
            ITwoFactorAuthenticationService twoFactorAuth)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _useTwoFactorAuth = options.Value.UseTwoFactorAuth;
            _twoFactorAuth = twoFactorAuth;
        }

        // Get all users
        [HttpGet("users")]
        public async Task<ActionResult<List<DTOUserData>>> GetAllUsers()
        {
            var users = await _userManager.Users.Select(u => new DTOUserData
            {
                Username = u.UserName,
                Email = u.Email
            }).ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }


        // Get a user by ID
        [HttpGet("user/{id}")]
        public async Task<ActionResult<DTOUserData>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userData = new DTOUserData
            {
                Username = user.UserName,
                Email = user.Email
            };

            return Ok(userData);
        }


        // Update user
        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUser(string id, DTOUpdateUser userUpdate)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check for username uniqueness, if it is provided in the request and is different from the current.
            if (userUpdate.UserName != null && userUpdate.UserName != user.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(userUpdate.UserName);
                if (existingUser != null)
                {
                    return Conflict("Username is already taken.");
                }

                user.UserName = userUpdate.UserName;
            }

            // Check for email uniqueness, if it is provided in the request and is different from the current.
            if (userUpdate.UserEmail != null && userUpdate.UserEmail != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(userUpdate.UserEmail);
                if (existingUser != null)
                {
                    return Conflict("Email is already taken.");
                }

                user.Email = userUpdate.UserEmail;
                // More logic for email confirmation should be added here.
            }

            // Save the changes
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            // If we've gotten this far, something failed.
            return BadRequest(result.Errors);
        }


        // Delete user
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            throw new NotImplementedException();
        }

        // Add a role to a user
        [HttpPost("user/{id}/role")]
        public async Task<IActionResult> AddRoleToUser(string id, string role)
        {
            throw new NotImplementedException();
        }

        // Remove a role from a user
        [HttpDelete("user/{id}/role/{role}")]
        public async Task<IActionResult> RemoveRoleFromUser(string id, string role)
        {
            throw new NotImplementedException();
        }

        // Get all roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            throw new NotImplementedException();
        }

        // Create role
        [HttpPost("role")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        //public AdminController(IOptions<SecuritySettings> options,
        //    ITwoFactorAuthentication twoFactorAuthentication,
        //    UserManager<User> userManager)
        //{
        //    _useTwoFactorAuth = options.Value.UseTwoFactorAuth;
        //    _twoFactorAuthentication = twoFactorAuthentication;
        //    _userManager = userManager;
        //}

        [HttpGet("request2fa")]
        public async Task<IActionResult> RequestTwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null) return BadRequest();

            var result = _twoFactorAuth.SendVerificationCodeAsync(user);
            return Ok(new { message = "2FA sent." });
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test(string userId, string token)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return BadRequest(new { message = "User error!" });
            }

            if (_useTwoFactorAuth)
            {
                if (!_twoFactorAuth.ConfirmVerificationCodeAsync(user, token))
                {
                    return Unauthorized();
                }
            }

            // If we get here the user is authenticated properly.

            return Ok();
        }
        // Delete role
        [HttpDelete("role/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
