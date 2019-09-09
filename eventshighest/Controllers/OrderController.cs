using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eventshighest.Data;
using eventshighest.Interface;
using eventshighest.Model;
using eventshighest.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eventshighest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        public OrderController(IOrderRepository orderRepository, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager,IJwtTokenService jwtTokenService)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }
        // GET: api/Order

        // GET: api/Order/5
        [HttpGet]
        public async Task<IActionResult> Get(int id,int userid)
        {
            return Ok(new { Order = await _orderRepository.Getorder(id), token = _jwtTokenService.CreateToken(await _userManager.FindByIdAsync(userid.ToString())) });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>GetActivityOrders(int id, [FromQuery]UrlQuery query,DateTime startdatetime,DateTime enddatetime,int PaymentStatus)
        {
            return Ok(await _orderRepository.Getorders(id,startdatetime,enddatetime,query.SearchQuery,query.PageSize,query.PageNumber.Value,PaymentStatus));
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> Createorder([FromBody] CreateorderPayload value)
        {
            if (ModelState.IsValid)
            {            
                int orderid;
                var sessionId = HttpContext.Session.Id;
                if (User.Identity.Name != null)
                {
                    int userid = int.Parse(User.Identity.Name);
                    orderid = await _orderRepository.CreateOrder(userid, value);
                    return RedirectToAction("Get", new { id = orderid, userid = userid });
                }
              
                else
                {
                    var user = new AppUser { UserName = Guid.NewGuid().ToString() };
                    //var Usercreated = await _userManager.CreateAsync(user, Guid.NewGuid().ToString());
                    orderid = await _orderRepository.CreateOrder(user.Id, value);
                    return RedirectToAction("Get", new { id = orderid, userid = user.Id });
                }        
            }
            else
            {
                return BadRequest(modelState: ModelState);
            }           
        }

        // PUT: api/Order/5
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
