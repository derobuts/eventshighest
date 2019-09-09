using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        IBasketRepository _basketRepository;
        public CartController(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }
        // GET: api/Basket
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Basket/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Basket
        [HttpPost]
        public void Post([FromBody] CustomerBasket basket)
        {
            if (User.Identity.Name != null)
            {
                _basketRepository.UpdateBasketAsync(basket);
            }
            var option = new CookieOptions();
            option.Expires = DateTime.Now.AddHours(1);

            Response.Cookies.Append("",);
        }

        // PUT: api/Basket/5
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
