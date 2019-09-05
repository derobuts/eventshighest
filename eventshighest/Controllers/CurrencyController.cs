using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eventshighest.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrency _currency;
        public CurrencyController (ICurrency currency)
        {
            _currency = currency;
        }
        // GET: api/Currency
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Currency/5
        [HttpGet]
        public async Task<IActionResult> Getcurrencies()
        {
            return Ok(await _currency.Getcurrencies());
        }

        // POST: api/Currency
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Currency/5
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
