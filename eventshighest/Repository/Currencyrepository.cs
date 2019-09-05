using Dapper;
using eventshighest.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class CurrencyRepository : BaseRepository, ICurrency
    {
        public readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        public CurrencyRepository(IConfiguration config, IHttpClientFactory httpClientFactory) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }
        public async Task Getexchangeratescurrencies()
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var apikey = _config["fixer.io:Apikey"];
            var httpresponse = await _httpClient.GetAsync($"http://data.fixer.io/api/latest?access_key={apikey}&base=EUR&symbols=USD,KES,NGN,GHS,TZS,ZAR,UGX,GBP,EUR,RWF");
            var data = await httpresponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<CurrencyRatesResponce>(await httpresponse.Content.ReadAsStringAsync());
            Type type = response.rates.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                //var Currencyratefrombase = new { Code = property.Name, Rate = property.GetValue(response.rates) };
                await Addcurrency(response.@base, property.Name, (decimal)property.GetValue(response.rates));
            }
        }
        public async Task Addcurrency(string BaseCode,string Code, decimal Rate)
        {
           
            //const string Sql = "update Currencyrates set Rate = @rate,Date_from = Getutcdate() where Tocurrencyid = (select Codeid from Currency where Code = @code)";
            const string Sql = @"insert into Currencyrates(Basecurrencyid,Tocurrencyid,Rate,Date_from,Date_to)" +
                                "values((select Codeid from Currency where Code = @basecurrency),(select Codeid from Currency where Code = @code),@rate,@datefrom,@dateto)";
            await WithConnection2(async c =>
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@basecurrency",BaseCode, DbType.String);
                parameters.Add("@code", Code, DbType.String);
                parameters.Add("@rate", Rate, DbType.Decimal);
                parameters.Add("@datefrom",DateTime.UtcNow.Date, DbType.Date);
                parameters.Add("@dateto",DateTime.UtcNow.AddDays(5).Date,DbType.Date);
                await c.ExecuteAsync(Sql, parameters, commandType: CommandType.Text);
            });
        }

        public async Task<dynamic> Getcurrencies()
        {
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<dynamic>("select Code from Currency",
                   commandType: CommandType.Text
                   );
            });
        }
        public async Task<bool> CheckcurrencyexistsAsync(string code)
        {
            return await WithConnection(async c =>
            {
                var exists = await c.QueryFirstAsync<string>("select Code from Currency where Code = @code", new { @code = code }, commandType: CommandType.Text);
                return exists == null ? false : true;
            });
        }
        public class CurrencyRatesResponce
        {
            public bool success { get; set; }
            public int timestamp { get; set; }
            public string @base { get; set; }
            public string date { get; set; }
            public Rates rates { get; set; }
        }
        public class Rates
        {
            public decimal USD { get; set; }
            public decimal KES { get; set; }
            public decimal NGN { get; set; }
            public decimal GHS { get; set; }
            public decimal TZS { get; set; }
            public decimal ZAR { get; set; }
            public decimal UGX { get; set; }
            public decimal GBP { get; set; }
            public decimal EUR { get; set; }
            public decimal AUD { get; set; }
        }
        public class Currencyratefrombase
        {
            public string Code { get; set; }
            public decimal Rate { get; set; }
        }
    }
}

