using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using static Dapper.SqlMapper;

namespace eventshighest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IVenueRepository _venueRepository;
        public readonly IConfiguration _config;
        public LocationController (IVenueRepository venueRepository,IConfiguration config)
        {
            _venueRepository = venueRepository;
            _config = config;
        }
        // GET: api/Location
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]UrlQuery urlQuery)
        {
            var result = await _venueRepository.ActivitiesinCity(urlQuery.cityname, urlQuery.PageSize, urlQuery.PageNumber.Value);
            if (result.Any())
            {
                return Ok(new {pagingend = false,nextpage = $"{Request.Path}?{urlQuery.PageNumber}&{urlQuery.PageSize}", activitiescity = result });
            }
            return Ok(new { pagingend = true, nextpage = $"{Request.Path}?{urlQuery.PageNumber}&{urlQuery.PageSize}", activitiescity = result });
        }

        // GET: api/Location/5
        [HttpGet("Topcities")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _venueRepository.Popularcities());
        }

        // POST: api/Location
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Location/5
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
