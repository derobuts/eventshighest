using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using eventshighest.Model;
using eventshighest.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        // GET: api/Pdf
        [HttpGet]

        public async Task<IActionResult>Get()
        {
            return new Rotativa.AspNetCore.ViewAsPdf("OrderConfirmation");   
        }

        // GET: api/Pdf/5
        [HttpGet("{id}")]
        public async Task<string> Get(int id)
        {
            return "";
        }

        // POST: api/Pdf
        [HttpPost]
        public async Task<string> Post([FromBody] object value)
        {
            var k = new Rotativa.AspNetCore.ViewAsPdf("OrderConfirmation",JsonConvert.DeserializeObject<Paidorder>(value.ToString()));
            return Convert.ToBase64String(await k.BuildFile(ControllerContext));
        }

        // PUT: api/Pdf/5
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
