using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactions _transactions;
        public readonly IConfiguration _config;
        public TransactionsController(ITransactions transactions, IConfiguration config)
        {
            _transactions = transactions;
            _config = config;
        }
        // GET: api/Transactions
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Transactions/5
        [HttpGet]
        public async Task<IActionResult> Gettransactions([FromQuery]UrlQueryTx urlQueryTx)
        {
            if (ModelState.IsValid)
            {
                var Usertx = await _transactions.GetTransactions(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), urlQueryTx.Startdate.ToUniversalTime(), urlQueryTx.Enddate.ToUniversalTime(), urlQueryTx.Type, urlQueryTx.Status, urlQueryTx.PageNumber.Value, urlQueryTx.PageSize, urlQueryTx.Query);
                if (Usertx.Any())
                {
                    return Ok(new { paginationend = false, nextpage = $"&PageNumber={urlQueryTx.PageNumber + 1}&PageSize{urlQueryTx.PageSize}", transactions = Usertx });
                }
                else
                {
                    return Ok(new { paginationend = true });
                }
            }
            return BadRequest();
        }
        [HttpGet("{currency}")]
        public async Task<IActionResult> Getuserbalancenetpayouts(string currency)
        {
            return Ok(await _transactions.UserBalanceNetPayouts(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),currency));
        }

        // POST: api/Transactions
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Transactions/5
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
