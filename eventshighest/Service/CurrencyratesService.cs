using eventshighest.Interface;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    public class CurrencyratesService : BackgroundService
    {
        private readonly ICurrency _currencyrates;
        public CurrencyratesService(ICurrency currencyrates)
        {
            _currencyrates = currencyrates;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (false == stoppingToken.IsCancellationRequested)
            {
                await _currencyrates.Getexchangeratescurrencies();
                await Task.Delay(18000000);
            }
        }
    }
}
