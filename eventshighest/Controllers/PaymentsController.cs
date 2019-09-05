using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eventshighest.Interface;
using eventshighest.Model;
using eventshighest.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IpaymentsRepository _ipaymentsRepository;
        public PaymentsController(IpaymentsRepository ipaymentsRepository)
        {
            _ipaymentsRepository = ipaymentsRepository;
        }
        // GET: api/Payments
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet]
        public async Task<IActionResult> Getuserbankaccounts()
        {
            var k = User.Identity.IsAuthenticated;
            return Ok(await _ipaymentsRepository.Getuserbankaccounts(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)));
        }

        // POST: api/Payments
        [HttpPost]
        public async Task<IActionResult> Post(BankAccount bankAccount)
        {
            bool H = User.Identity.IsAuthenticated;
            string Hy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            int userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok(await _ipaymentsRepository.AddPayoutAccount(bankAccount, int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)));
        }
        [HttpPost]
        public async Task<IActionResult> Withdrawalrequest(WithdrawalviewModel withdrawalviewmodel)
        {
            int O = await _ipaymentsRepository.Requestwithdrawalpayment(withdrawalviewmodel, int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            if (O == 0)
            {
                return BadRequest();
            }
            return Ok();
        }

        // PUT: api/Payments/5
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
