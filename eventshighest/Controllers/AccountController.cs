using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using eventshighest.Data;
using eventshighest.Interface;
using eventshighest.Service;
using eventshighest.ViewModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IJwtTokenService _jwtTokenService;
        public AccountController(
           Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager, IJwtTokenService jwtTokenService,
           SignInManager<AppUser> signInManager,IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
           _jwtTokenService = jwtTokenService;
            _emailService = emailService;
        }
        // GET: api/Account
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Account/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Account
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Ok(_jwtTokenService.CreateToken(user));
            }
            return Unauthorized();
        }
        [AllowAnonymous]
        [HttpPost]      
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var appUser = await _userManager.FindByEmailAsync(model.Email);
                if (appUser != null)
                {
                    return BadRequest("Email already in Use");
                }
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                //check if user with Email Exists               
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await SendUserEmailVerificationAsync(user);
                    return Ok(_jwtTokenService.CreateToken(user));
                }
                return BadRequest();
            }
            return BadRequest();
        }
        [AllowAnonymous]
        [HttpGet]        
        public async Task<IActionResult>ConfirmEmail(int? userId, string emailtoken)
        {
            if (userId == null || emailtoken == null)
            {
                return Content("Bad Request");
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            emailtoken = emailtoken.Replace("%2f", "/").Replace("%2F", "/");
            //verify email token
            var result = await _userManager.ConfirmEmailAsync(user, emailtoken);
            return Ok(result.Succeeded ? "Email Verified" : "Error Verify Email");
        }
        private async Task SendUserEmailVerificationAsync(AppUser user)
        {
            // Get the user details
            var userIdentity = await _userManager.FindByIdAsync(user.Id.ToString());
            // Generate an email verification code
            var emailVerificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var c = HttpUtility.UrlEncode(emailVerificationCode);
            // TODO: Replace with APIRoutes that will contain the static routes to use
            var confirmationUrl = $"http://{Request.Host.Value}/api/Account/ConfirmEmail/?userId={userIdentity.Id.ToString()}&emailtoken={HttpUtility.UrlEncode(emailVerificationCode)}";
            // Email the user the verification code
            await _emailService.SendEmailAsync(user.Email,"Account Verification",confirmationUrl, "d-971e5d4b691042299c524bb74dd8f6c0");
        }

        // PUT: api/Account/5
       [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
