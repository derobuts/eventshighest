﻿using eventshighest.Interface;
using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class BasketRepository : IBasketRepository
    {
        public Task<bool> DeleteBasketAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerBasket> GetBasketAsync(string customerId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {
            throw new NotImplementedException();
        }
    }
}
