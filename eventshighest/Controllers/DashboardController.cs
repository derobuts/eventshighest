using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eventshighest.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }
        // GET: api/Dashboard
        [HttpGet]
        public async Task<IActionResult> GetActivitystats(int activityid)
        {
           // string s = User.FindFirst(ClaimTypes.NameIdentifier).Value;
           // int userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return new ObjectResult(await _dashboardRepository.GetActivitystats(activityid));
        }
        [HttpGet]
        public async Task<IActionResult> Getuserbalance()
        {
            // string s = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            // int userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return new ObjectResult(await _dashboardRepository.UserBalance(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)));
        }
        // GET: api/Dashboard/5
        [HttpGet]
        public async Task<IActionResult> Getuseractivities([FromQuery]int activitystatus)
        {
            return new ObjectResult(await _dashboardRepository.Getuseractivities(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),activitystatus));
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Getactivitycurrencies()
        {
            return new ObjectResult(await _dashboardRepository.Currencies());
        }

        // POST: api/Dashboard
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT: api/Dashboard/5
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
