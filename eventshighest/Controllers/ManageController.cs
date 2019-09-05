using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using eventshighest.Data;
using eventshighest.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ManageController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;
        public ManageController(UserManager<AppUser> userManager,
         SignInManager<AppUser> signInManager,
         UrlEncoder urlEncoder
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
        }
        // GET: api/Manage
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Manage/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        

        // POST: api/Manage
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
        
        // PUT: api/Manage/5
        [HttpPut]
        public async Task<IActionResult>Put([FromBody] Guestuser value)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                user.Email = value.Email;
                user.PhoneNumber = value.Phoneno;
                var result = await _userManager.UpdateAsync(user);
            }
            return Ok();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
