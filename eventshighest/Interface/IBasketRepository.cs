﻿using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IBasketRepository
    {
        Task<CustomerBasket> GetBasketAsync(string customerId);
        IEnumerable<string> GetUsers();
        Task<CustomerBasket>UpdateBasketAsync(CustomerBasket basket);
        Task<bool> DeleteBasketAsync(string id);
    }
}
