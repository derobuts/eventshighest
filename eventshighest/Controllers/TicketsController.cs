using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eventshighest.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketclass _ticketclass;
        private readonly ICurrency _currency;
        public TicketsController(ITicketclass ticketclass, ICurrency currency)
        {
            _ticketclass = ticketclass;
            _currency = currency;
        }
        // GET: api/Tickets
        [HttpGet("{activityid}")]
        public async Task<IActionResult> Get(int activityid)
        {
            
            return new ObjectResult(await _ticketclass.GetactivityticketClasses(activityid,"KES"));
        }

        [HttpGet("Checkticketsavailability")]
        public async Task<IActionResult>Get([FromQuery]int activityid,DateTime activitydate,string currency)
        {
            if (currency == null)
            {
                currency = "USD";
                return new ObjectResult(await _ticketclass.Getactivitydateticketstatus(activityid, activitydate, currency));
            }
            var currencyexists = await _currency.CheckcurrencyexistsAsync(currency);         
            currency = currencyexists == true ? currency : "USD";
            return new ObjectResult(await _ticketclass.Getactivitydateticketstatus(activityid,activitydate,currency));
        }
        // POST: api/Tickets
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT: api/Tickets/5
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
