using IdaWebApplicationTemplate;
using IdaWebApplicationTemplate.Data;
using IdaWebApplicationTemplate.Services;
using IdaWebApplicationTemplate.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace IdaWebApplicationTemplate.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;

        public UserController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IEmailService emailService)            
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }


        /// <summary>
        /// Checks the authentication and lockout status of the current user.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the user is not authenticated, does not exist, or is locked out.</exception>
        private async Task CheckUserStatus()
        {
            // Retrieves the user's unique identifier from the authenticated user's claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Throws an exception if the user is not authenticated
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User is not authenticated");
            }

            // Fetches the user's details using the user's unique identifier
            var user = await _userManager.FindByIdAsync(userId);

            // Throws an exception if the user does not exist
            if (user == null)
            {
                throw new Exception("User does not exist");
            }

            // Checks whether the user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                // Throws an exception if the user is locked out
                throw new Exception("User is locked out");
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> CreateToken([FromBody] DTOLogin dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var claim = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    expires: DateTime.UtcNow.AddHours(1),
                    claims: claim,
                    signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256));

                var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,  // Cookie will only be sent over HTTPS
                    SameSite = SameSiteMode.None,  // Cookie will be sent on cross-origin requests
                };

                Response.Cookies.Append("JWT_Cookie", tokenStr, cookieOptions);

                return Ok();  // Token is not sent in the response body anymore
            }
            return Unauthorized();
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout(DTOConfirmEmail dto)
        {
            // The claim identity contains the userId from JWT token
            var claimIdentity = this.User?.Identity as ClaimsIdentity;
            if (claimIdentity == null)
            {
                // User is not authenticated, cannot log out
                return Unauthorized();
            }

            var userIdClaim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // User does not have a valid NameIdentifier claim, cannot log out
                return Unauthorized();
            }

            var userId = userIdClaim.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                // User ID is not valid, cannot log out
                return Unauthorized();
            }

            // Log the logout activity
            // You may have your own logging mechanism in place
            // For now, I'll just use a placeholder function LogUserActivity
            Console.WriteLine("Logged out: " + userId);

            return Ok();
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<DTOUserData>> GetCurrentUser()
        {
            foreach (var item in HttpContext.User.Claims)
            {
                Console.WriteLine("Claim: " + item);
            }
            
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                // Not logged in
                return Unauthorized();
            }

            return new DTOUserData()
            { 
                Username = user.UserName,
                Email = user.Email
            };
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(DTORegisterUser dto)
        {
            var userExists = await _userManager.FindByNameAsync(dto.UserName) != null
                             || await _userManager.FindByEmailAsync(dto.Email) != null;

            if (userExists)
                return BadRequest(new { message = "Username or Email already in use." });

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Here, you should format your email content and send the email with the token.
            // Please replace the url variable value with your real confirmation URL
            var url = $"https://localhost:7231/api/user/confirmemail?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";
            var emailSendResult = await _emailService.SendEmailAsync("Confirm your email", $"Please confirm your account by clicking this link: {url}", dto.Email);

            return Ok(new { message = "Registration successful, please check your email to confirm your account." });
        }



        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Email confirmation succeeded." });
        }

        [HttpGet("update")]
        [Authorize]
        public async Task<ActionResult<User>> Update(DTOUpdateUser dto)
        {
            try
            {
                await CheckUserStatus();

                // User is not locked out.


            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }

            return Ok( new { message = "No user editable fields."});
        }

        [HttpPost("resetpassword")]
        public async Task<ActionResult> RequestResetPasswordEmail([FromBody] DTOResetPassword resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user != null)
            {
                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Create a URL which includes the token.
                var resetLink = Url.Action("ResetPassword", "Account", new { token }, protocol: HttpContext.Request.Scheme);

                // Create the email content.
                string emailTitle = "Password Reset Request";
                string emailContent = $"Please reset your password by <a href='{resetLink}'>clicking here</a>. If you did not request a password reset, please ignore this email.";

                // Send the password reset email.
                await _emailService.SendEmailAsync(emailTitle, emailContent, user.Email);
            }

            // Always return the same message regardless of the email was found or not. 
            // This is to prevent enumerating existing emails based on server responses.
            return Ok("If your account exists, you will receive an email to reset your password.");
        }

        [HttpPost("confirmresetpassword")]
        public async Task<ActionResult> ConfirmResetPassword([FromBody] DTOConfirmResetPassword dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist.
                return BadRequest("Failed to reset password.");
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (result.Succeeded)
            {
                return Ok("Your password has been successfully reset.");
            }

            // If we got this far, something failed.
            return BadRequest("Failed to reset password.");
        }

        [HttpPost("deleteuser")]
        [Authorize]
        public async Task<ActionResult> DeleteUser([FromBody] DTODeleteUser dto)
        {
            try
            {
                await CheckUserStatus();

                // User is not locked out.

                if (!dto.Delete)
                {
                    return BadRequest("Failed to delete user.");
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Verify if the password is correct
                var passwordVerification = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
                if (passwordVerification.Succeeded)
                {
                    // Delete the user
                    var deleteResult = await _userManager.DeleteAsync(user);
                    if (deleteResult.Succeeded)
                    {
                        Console.WriteLine($"User with ID {user.Id} deleted successfully.");
                        return Ok("User deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to delete user with ID {user.Id} due to: {string.Join(", ", deleteResult.Errors.Select(x => x.Description))}");
                        return BadRequest("Failed to delete user.");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to verify password for user with ID {user.Id}.");
                    return BadRequest("Invalid password.");
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            
        }
    }
}
