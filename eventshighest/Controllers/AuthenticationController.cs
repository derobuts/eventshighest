using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eventshighest.Data;
using eventshighest.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace eventshighest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        public AuthenticationController(IJwtTokenService jwtTokenService, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _jwtTokenService = jwtTokenService;
            _userManager = userManager;
        }
        // GET: api/Authentication
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Authentication/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Authentication
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT: api/Authentication/5
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
