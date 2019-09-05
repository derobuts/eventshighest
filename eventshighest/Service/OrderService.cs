using Dapper;
using eventshighest.Controllers;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QRCoder;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace eventshighest.Service
{
    public class OrderService : BackgroundService
    {

        public readonly IEmailService _emailService;
        //public readonly IEmailService _emailService;
        public readonly IBackgroundTaskQueue _queueService;
        private readonly HttpClient _httpClient;
        private IConfiguration _configuration { get; set; }
        static HttpClient client = new HttpClient();

        public OrderService(IEmailService emailService, IConfiguration configuration, IBackgroundTaskQueue queueService,IHttpClientFactory httpClientFactory)
        {
            _emailService = emailService;
            _configuration = configuration;
            _queueService = queueService;
            _httpClient = httpClientFactory.CreateClient();
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (false == stoppingToken.IsCancellationRequested)
            {
                await CheckOrderspaidtoemail(stoppingToken);               
                await Task.Delay(5000);
            }
        }     
        public async Task CheckOrderspaidtoemail(CancellationToken cancellationToken)
        {
            IEnumerable<Paidorder> Paidorders;
            using (var c = new SqlConnection(_configuration["Dbconstring:dbConnectionString"]))
            {
                c.Open();
                Paidorders = await c.QueryAsync<Paidorder>("Getpaidorders", commandType: CommandType.StoredProcedure);
            }
            foreach (var paidorders in Paidorders)
            {
                paidorders.orderItems = GetItemsinOrder(paidorders.OrdersId);
                foreach (var item in paidorders.orderItems)
                {
                    item.Barcodeimgbase64 = QRCodegenerator(item.Barcode);
                }
            }
                       
            foreach (var paidorder in Paidorders)
            {
                var response = await _httpClient.PostAsJsonAsync("http://localhost:44310/api/pdf/Post", paidorder);
                var attachment = await response.Content.ReadAsStringAsync();
                var delivered = await _emailService.SendEmailAsync(paidorder.Email, "Order", "order items", "d-1a64faeb0b174e8bbe1c88d2a7e93ff9", attachment);

                if (delivered)
                {
                    using (var c = new SqlConnection(_configuration["Dbconstring:dbConnectionString"]))
                    {
                        c.Open();
                        await c.ExecuteAsync(@"update Orders set Delivery_status = 105
					                        where OrdersId = @orderid", new { @orderid = paidorder.OrdersId });
                    }
                }            
            }       
        }
        public IEnumerable<PaidOrderItems> GetItemsinOrder(int orderid)
        {
            using (var c = new SqlConnection(_configuration["Dbconstring:dbConnectionString"]))
            {
                c.Open();
                try
                {
                   
                    const string Sql = @"select Top 100 a.Name,tc.Name,t.Barcode,(select Start_datetime from Activity_occurrence where Activity_occurrence_id = o.Activity_occurrence) as Startdate,
             (select End_datetime from Activity_occurrence where Activity_occurrence_id = o.Activity_occurrence) as Enddate,v.PlaceAddress
             from Orders o 
             inner join Ticket t on t.Orderid = o.OrdersId
			 inner join Ticketclass tc on tc.Ticket_id = t.Ticketclass_id
			 inner join Activity a on a.Activity_id = tc.Activity_id
			 inner join Venue v  on v.Venue_id = a.Venue_id
             where  o.OrdersId = @orderid and o.Status = 101 and o.Delivery_status = 103
			 order by o.Created desc";
                    return c.Query<PaidOrderItems>(Sql, new { @orderid = orderid },commandType:CommandType.Text);
                }
                catch (Exception ex)
                {
                    var h = ex;
                    throw;
                }               
            }
        }
        private string QRCodegenerator(string ticketbarcode)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qrGenerator.CreateQrCode(ticketbarcode, QRCodeGenerator.ECCLevel.M);
            //create byte/raw bitmap qr code
            BitmapByteQRCode qrCodeBmp = new BitmapByteQRCode(qRCodeData);
            byte[] qrCodeImageBmp = qrCodeBmp.GetGraphic(4);
            //var h = Convert.ToBase64String(qrCodeImageBmp);
            var qrurl = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeImageBmp));
            return qrurl;
        }
    }
}
