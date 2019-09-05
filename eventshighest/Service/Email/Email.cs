using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace eventshighest.Service
{

    public class Email : IEmailService
    {
        Paidorder Ordertodeliver;
        
        private IConfiguration _configuration { get; set; }
        public Email(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SetEmailpayoad(Paidorder paidorder)
        {
            Ordertodeliver = paidorder;
        }
        public async Task sendEmail(CancellationToken cancellationToken)
        {
            try
            {
                var client = new SendGridClient(_configuration["Sendgrid:Apikey"]);
                var msg = new SendGridMessage();
                msg.SetFrom(new SendGrid.Helpers.Mail.EmailAddress(""));
                msg.AddTo(new SendGrid.Helpers.Mail.EmailAddress("tbutoyez@gmail.com"));
                msg.SetTemplateId("d-1a64faeb0b174e8bbe1c88d2a7e93ff9");
                var dynamicTemplateData = new
                {
                    OrderBought = new
                    {
                        Ordertodeliver.orderItems
                    }
                };
                var h = JsonConvert.SerializeObject(dynamicTemplateData);
                msg.SetTemplateData(dynamicTemplateData);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    using (var c = new SqlConnection())
                    {
                        c.Open();
                        await c.ExecuteAsync(@"update Orders set Delivery_status = 101
					                        where OrdersId = @orderid", new { @orderid = Ordertodeliver.OrdersId });
                    }
                }
            }
            catch (Exception ex)
            {

                // throw;
            }

        }

        public async Task<bool> SendEmailAsync(string emailto,string subject, object message, string templateid=null,string attachmentsfile = null)
        {
            var client = new SendGridClient(_configuration["Sendgrid:Apikey"]);
            var msg = new SendGridMessage();
            msg.SetFrom(new SendGrid.Helpers.Mail.EmailAddress(_configuration["Sendgrid:Emailfrom"]));
            msg.AddTo(new SendGrid.Helpers.Mail.EmailAddress(emailto));
            //body = "Email Body";
            msg.Subject = subject;
            if (templateid != null)
            {
                msg.SetTemplateId(templateid);             
                var h = JsonConvert.SerializeObject(new { templatedata = message });
                msg.SetTemplateData(new { templatedata = message});
            }
            if (attachmentsfile != null)
            {
                msg.AddAttachment("orderreceipt.pdf", attachmentsfile, "application/pdf");
                /**
                msg.AddAttachment(
                    new Attachment
                    {
                        Filename = "orderreceipt",
                        Content = attachments,
                        Type = MimeType.Text.,
                        Disposition = "attachment",
                        ContentId = Guid.NewGuid().ToString()
                    }
                    );**/
            }
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == HttpStatusCode.Accepted ? true : false;
        }
    }
}
