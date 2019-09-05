using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eventshighest.Controllers.Repository;
using eventshighest.Interface;
using eventshighest.Model;
using eventshighest.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ICurrency _currency;
        public ActivityController(IActivityRepository activityrepository, ICurrency currency)
        {
            _activityRepository = activityrepository;
            _currency = currency;
        }
        [HttpGet("getactivitycategories")]
        public async Task<IActionResult> Get()
        {
            return new ObjectResult(await _activityRepository.GetActivityCategories());
        }

        [HttpGet("gettopnearby")]
        [AllowAnonymous]
        public async Task<IActionResult> Gettopnearby(string country, string currencycode)
        {
            if (currencycode == null)
            {
                currencycode = "USD";
                return new ObjectResult(new { popularnearby = await _activityRepository.PopularNearby(country, currencycode), popular = await _activityRepository.Gettopactivities(currencycode) });
            }
            var currencyexists = await _currency.CheckcurrencyexistsAsync(currencycode);
            currencycode = currencyexists == true ? currencycode : "USD";
            return new ObjectResult(new {popularnearby = await _activityRepository.PopularNearby(country, currencycode),popular = await _activityRepository.Gettopactivities(currencycode)});
        }
       

        [HttpGet("getactivitydates")]
        [AllowAnonymous]
        
        public async Task<IActionResult>GetActivitydates(int activityid)
        {
            return new ObjectResult(await _activityRepository.Getactivityavailabledates(activityid));
        }
        [HttpGet("getactivitydetails")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivitydetail(int activityid)
        {
            return new ObjectResult(await _activityRepository.Getactivitydetails(activityid));
        }
        [HttpGet("getactivitytickets")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivitytickets(int activityid)
        {
            return new ObjectResult(await _activityRepository.Getactivitydetails(activityid));
        }
        [HttpPost("addactivity")]
        public async Task Post([FromBody]Activity value)
        {
            try
            {
                await _activityRepository.AddActivity(value, int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            }
            catch (Exception ex)
            {
                var h = ex;
                throw;
            }

        }
    }
}