using Dapper;
using eventshighest.Interface;
using eventshighest.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    
    public class PaymentsService : BackgroundService
    {
        private readonly IPaymentsBackgroundQueue _backgroundTaskQueue;
        private IConfiguration _configuration { get; set; }
        private readonly HttpClient _httpClient;
        public PaymentsService(IPaymentsBackgroundQueue backgroundTaskQueue,IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (false == stoppingToken.IsCancellationRequested)
            {
                //_backgroundTaskQueue.QueueBackgroundWorkItem(Withrawalrequests);
                await Withrawalrequests(stoppingToken);
                await Task.Delay(5000);
            }
        }
        public async Task Withrawalrequests(CancellationToken cancellationToken)
        {
            IEnumerable<PayoutTransfer> Payoutstosend;
            using (var c = new SqlConnection(_configuration["Dbconstring:dbConnectionString"]))
            {
                c.Open();
                Payoutstosend = await c.QueryAsync<PayoutTransfer>("Processpayouts", commandType: CommandType.StoredProcedure);
            }
            foreach (var payout in Payoutstosend)
            {
                payout.seckey = _configuration["flutterwave:Secret Key"];
                _httpClient.DefaultRequestHeaders.Add("Accept","application/json");
                var stringContent = new StringContent(JsonConvert.SerializeObject(payout), Encoding.UTF8,"application/json");
                var httpresponse = await _httpClient.PostAsync("https://api.ravepay.co/v2/gpx/transfers/create", stringContent);
                var response = JsonConvert.DeserializeObject<ResponseData>(await httpresponse.Content.ReadAsStringAsync());
                if (httpresponse.IsSuccessStatusCode  && (response.status == "success"))
                {
                    using (var c = new SqlConnection(_configuration["Dbconstring:dbConnectionString"]))
                    {
                        c.Open();
                        try
                        {
                            string Sql = $"update TransactionHistory set PaymentStatus = @status where id = @id and Code = @code";
                            var kl = payout.reference.Split(':')[1];
                            // string sql = $"INSERT INTO Transactiontx (Code,Userid,Amount,PaymentMethod,Txoperation,PaymentStatus,Date,Order_Id,Currency)values({response.data.id},{userid},{amountsent},{response.data.bank_name},{101},{100},{DateTime.UtcNow},{1},{response.data.currency})";
                            await c.ExecuteAsync(Sql, new { @status = 103, @id = payout.reference.Split(':')[0], @code = payout.reference.Split(':')[1] }, commandType: CommandType.Text);
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }                    
                    }
                }
                else
                {
                    using (var c = new SqlConnection(_configuration["Dbconstring:dbConnectionString"]))
                    {
                        c.Open();
                        try
                        {
                            string Sql = $"update TransactionHistory set PaymentStatus = @status where id = @id and Code = @code";
                            var kl = payout.reference.Split(':')[1];
                            // string sql = $"INSERT INTO Transactiontx (Code,Userid,Amount,PaymentMethod,Txoperation,PaymentStatus,Date,Order_Id,Currency)values({response.data.id},{userid},{amountsent},{response.data.bank_name},{101},{100},{DateTime.UtcNow},{1},{response.data.currency})";
                            await c.ExecuteAsync(Sql, new { @status = 102, @id = payout.reference.Split(':')[0], @code = payout.reference.Split(':')[1] }, commandType: CommandType.Text);
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }
                    }
                }
            }
        }
    }
}
