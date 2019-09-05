using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eventshighest.Interface;
using eventshighest.Repository;
using eventshighest.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RavepaymentsController : ControllerBase
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IravePayments _Iravepayments;

        public RavepaymentsController(IBackgroundTaskQueue backgroundTaskQueue, IravePayments ravepayments)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _Iravepayments = ravepayments;
        }
        [HttpGet("GetListOfBanksforPayouts/{isocountry}")]
        public async Task<IActionResult> ListOfBanksforPayout(string isocountry)
        {
            return new ObjectResult(await _Iravepayments.BanksSupportingTransferinregion(isocountry));
        }
        // GET: api/Ravepayments
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Ravepayments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string isocountry)
        {
            return new ObjectResult(await _Iravepayments.BanksSupportingTransferinregion(isocountry));
        }

        [HttpGet("Addpayoutaccount")]
        public async Task<IActionResult>AddPayoutAccount([FromBody]WebHookPayLoad webHookPayLoad)
        {
            //write the transaction with a pending status to verify later
            //await new Transactions().AddUnverifiedTx(webHookPayLoad);
            _backgroundTaskQueue.QueueBackgroundWorkItem(async token => {

            });
            return Ok();
        }
        // POST: api/Ravepayments
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult>Post([FromBody]WebHookPayLoad webHookPayLoad)
        {
            //write the transaction with a pending status to verify later
            //await new Transactions().AddUnverifiedTx(webHookPayLoad);
            if (webHookPayLoad.EventType == "Transfer")
            {
                _backgroundTaskQueue.QueueBackgroundWorkItem(_Iravepayments.GetTransfersHook(webHookPayLoad.transfer));
            }
            else
            {
                _backgroundTaskQueue.QueueBackgroundWorkItem(_Iravepayments.GetFuncVerifyPayments(webHookPayLoad.txRef));             
            }
            return Ok();
        }
       

        // PUT: api/Ravepayments/5
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
