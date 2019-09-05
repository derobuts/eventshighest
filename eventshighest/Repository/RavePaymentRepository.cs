using Dapper;
using eventshighest.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class RavePaymentsRepository : BaseRepository, IravePayments
    {
        private readonly HttpClient _httpClient;
        private readonly IOrderRepository  _orderrepository;
        private IConfiguration _configuration { get; set; }
        public string Txref { get; set; }
        public Transfer transferpayload { get; set; }
        public RavePaymentsRepository(IHttpClientFactory httpClientFactory, IConfiguration configuration, IOrderRepository orderrepository) : base(configuration["Dbconstring:dbConnectionString"])
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _orderrepository = orderrepository;
        }
        public Func<CancellationToken, Task> GetFuncVerifyPayments(string txref)
        {
            Txref = txref;
            return VerifyPayments;
        }
        public Func<CancellationToken, Task> Getwithdrawalrequests()
        {
            
            return Completedeventstopayout;
        }
        public Func<CancellationToken, Task> GetTransfersHook(Transfer _transferpayload)
        {
            transferpayload = _transferpayload;
            return TransfersHook;
        }

        public async Task TransfersHook(CancellationToken cancellationToken)
        {
            if (transferpayload.status == "SUCCESSFUL")
            {
                await WithConnection2(async c =>
                {
                    string Sql = $"update TransactionHistory set PaymentStatus = @status,Narration = @narration where id = @id and Code = @code";
                    // string sql = $"INSERT INTO Transactiontx (Code,Userid,Amount,PaymentMethod,Txoperation,PaymentStatus,Date,Order_Id,Currency)values({response.data.id},{userid},{amountsent},{response.data.bank_name},{101},{100},{DateTime.UtcNow},{1},{response.data.currency})";
                    await c.ExecuteAsync(Sql, new { @status = 101, @id = transferpayload.reference.Split(':')[0], @code = transferpayload.reference.Split(':')[1] ,@narration = transferpayload.complete_message}, commandType: CommandType.Text);
                });               
            }
            else
            {
                await WithConnection2(async c =>
                {
                    string Sql = $"update TransactionHistory set PaymentStatus = @status,Narration = @narration where id = @id and Code = @code";
                    // string sql = $"INSERT INTO Transactiontx (Code,Userid,Amount,PaymentMethod,Txoperation,PaymentStatus,Date,Order_Id,Currency)values({response.data.id},{userid},{amountsent},{response.data.bank_name},{101},{100},{DateTime.UtcNow},{1},{response.data.currency})";
                    await c.ExecuteAsync(Sql, new { @status = 102, @id = transferpayload.reference.Split(':')[0], @code = transferpayload.reference.Split(':')[1], @narration = transferpayload.complete_message }, commandType: CommandType.Text);
                });
            }
        }


        public async Task VerifyPayments(CancellationToken cancellationToken)
        {
            //string txref = ;
            var data = new { txref = Txref, SECKEY = _configuration["flutterwave:Secret Key"] };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8,
                                    "application/json");
            var httpresponse = await _httpClient.PostAsync("https://api.ravepay.co/flwv3-pug/getpaidx/api/v2/verify", stringContent);
            var h = httpresponse.StatusCode;
            var jk = await httpresponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseData>(await httpresponse.Content.ReadAsStringAsync());
            if (response.data.status == "successful" && response.data.chargecode == "00")
            {
                await WithConnection2(async c =>
                {
                    await c.ExecuteAsync("update Orders set Status = 101 where OrdersId = @orderid", new { @orderid = response.data.meta.First().metavalue });
                });

                //await new OrderRespository().UpdateConfirmedOrder(Order.OrdersId);//update order and confrm it paid                   
                await WithConnection2(async c =>
                {
                    string sql = "insert into TransactionHistory(Order_Id,Amount,PaymentStatus,Txoperation,Currency,Date,Code)" +
                    "values(@orderid,@amount,@paymentstatus,@txoperation,@currency,@date,@txcode)";
                        //string sql = $"INSERT INTO Transactiontx (Code,Userid,Amount,PaymentMethod,Txoperation,PaymentStatus,Date,Order_Id,Currency)values({data.txref},{EventTenantID},{response.data.amount},{response.data.paymenttype},{100},{101},{DateTime.UtcNow},{Order.OrdersId},{response.data.currency})";
                        await c.ExecuteAsync(sql, new {@orderid = response.data.meta.First().metavalue,@amount = response.data.amount,@paymentstatus = 101,@txoperation = 100,@currency = response.data.currency,@date = DateTime.UtcNow , @txcode = data.txref })
                    ;
                });
            }
            else
            {
               
            }
        }
        public async Task AddTransferRecipientAccount(CreateTransferRecipient transferRecipient)
        {
            await WithConnection2(async c =>
            {
                string sql = "INSERT INTO BankAccounts(Userid,account_number,account_bank)values(@userid,@account_number,@account_bank)";
                await c.ExecuteAsync(sql, new { @userid = 1, @account_number = transferRecipient.account_number, @account_bank = transferRecipient.code, })
                ;
            });
            //log
        }

        public async Task CreateTransferRecipientAccount(CreateTransferRecipient transferRecipient)
        {
            var TransferClientAccount = new { account_number = transferRecipient.account_number, account_bank = transferRecipient.account_bank, seckey = _configuration["flutterwave:Secret Key"] };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var stringContent = new StringContent(JsonConvert.SerializeObject(TransferClientAccount), Encoding.UTF8,
                                    "application/json");
            var httpresponse = await _httpClient.PostAsync("https://api.ravepay.co/v2/gpx/transfers/beneficiaries/create", stringContent);
            var response = JsonConvert.DeserializeObject<ResponseData>(await httpresponse.Content.ReadAsStringAsync());
            if (response.data.status == "successful")
            {
                await WithConnection2(async c =>
                {
                    string sql = $"update BankAccounts Set id = @id where  account_number = {transferRecipient.account_number}";
                    // await c.ExecuteAsync(sql,new {response.data.id});
                });
            }
            //log
        }

        public async Task<IEnumerable<Bank>> BanksSupportingTransferinregion(string isocountrycode)
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var publickey = _configuration["flutterwave:Public Key"];
            var httpresponse = await _httpClient.GetAsync($"https://api.ravepay.co/v2/banks/{isocountrycode}?public_key={publickey}");
            var response = JsonConvert.DeserializeObject<Banksfortransferinregion>(await httpresponse.Content.ReadAsStringAsync());
            return response.listofbanksinregionfortransfer.Banks;
        }
        //
        public async Task Completedeventstopayout(CancellationToken cancellationToken)
        {
           
        }
        public async Task Updatetransactionstatusofeventpayout(TransferResponse transferResponse)
        {
            if (transferResponse.transfer.status == "success")
            {
                await WithConnection2(async c =>
                {
                    string sql = $"update Transactiontx set PaymentStatus = {101} where Code = {transferResponse.transfer.id}";
                    await c.ExecuteAsync(sql);
                });
                //update event as paid
                await WithConnection2(async c =>
                {
                    //event 0 still not paid out
                    //1 pending or in the process
                    //2 paid out
                    string sql = $"update Event set IsPaidout = {2} where Event_Id = {int.Parse(transferResponse.transfer.reference)}";
                    await c.ExecuteAsync(sql);
                });
            }
            else
            {

            }
        }

        public Task Completedeventstopayout()
        {
            throw new NotImplementedException();
        }
    }
    public class Bank
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class Listofbanksinregionfortransfer
    {
        public List<Bank> Banks { get; set; }
    }
    public class Banksfortransferinregion
    {
        public string status { get; set; }
        public string message { get; set; }
        [JsonProperty("data")]
        public Listofbanksinregionfortransfer listofbanksinregionfortransfer { get; set; }
    }
    public class Customer
    {
        public int id { get; set; }
        public string phone { get; set; }
        public string fullName { get; set; }
        public object customertoken { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public object deletedAt { get; set; }
        public int AccountId { get; set; }
    }
    public class Bulkpayments
    {
        public string seckey { get; set; }
        public string title { get; set; }
        public List<PayoutTransfer> bulk_data { get; set; }
    }
    public class CreateTransferRecipient
    {
        public int userid { get; set; }
        public string code { get; set; }
        public string account_number { get; set; }
        public string account_bank { get; set; }
    }
    public class PayoutTransfer
    {
        public string account_bank { get; set; }
        public decimal amount { get; set; }
        public string account_number { get; set; }
        public string seckey { get; set; }
        public string currency { get; set; }
        public string Narration { get; set; } = "Payouts";
        public string reference { get; set; }
        public string beneficiary_name { get; set; } = "Merchant";
    }
    public class Entity
    {
        public string id { get; set; }
        public string card6 { get; set; }
        public string card_last4 { get; set; }
    }

    public class WebHookPayLoad
    {
        public int id { get; set; }
        public string txRef { get; set; }
        public string flwRef { get; set; }
        public string orderRef { get; set; }
        public object paymentPlan { get; set; }
        public DateTime createdAt { get; set; }
        public int amount { get; set; }
        public double charged_amount { get; set; }
        public string reference { get; set; }
        public string status { get; set; }
        public string IP { get; set; }
        public string currency { get; set; }
        public Customer customer { get; set; }
        public Entity entity { get; set; }
        [JsonProperty("event.type")]
        public string EventType { get; set; }
        public Transfer transfer { get; set; }
    }
    public class CardToken
    {
        public string embedtoken { get; set; }
        public string shortcode { get; set; }
        public string expiry { get; set; }
    }
   
    public class Card
    {
        public string expirymonth { get; set; }
        public string expiryyear { get; set; }
        public string cardBIN { get; set; }
        public string last4digits { get; set; }
        public string brand { get; set; }
        public List<CardToken> card_tokens { get; set; }
        public string life_time_token { get; set; }
    }

    public class Meta
    {
        public int id { get; set; }
        public string metaname { get; set; }
        public string metavalue { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public object deletedAt { get; set; }
        public int getpaidTransactionId { get; set; }
    }

    public class Data
    {
        public int txid { get; set; }
        public string txref { get; set; }
        public string flwref { get; set; }
        public string devicefingerprint { get; set; }
        public string cycle { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string chargecode { get; set; }
        public string reference { get; set; }
        public string status { get; set; }
        public string paymenttype { get; set; }
        public object paymentplan { get; set; }
        public object paymentpage { get; set; }
        public string raveref { get; set; }
        // public Account account { get; set; }
        public List<Meta> meta { get; set; }
    }

    public class ResponseData
    {
        public string status { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
    }
    public class TransferResponse
    {
        [JsonProperty("event.type")]
        public string eventtype { get; set; }
        public Transfer transfer { get; set; }
    }
    public class Transfer
    {
        public int id { get; set; }
        public string account_number { get; set; }
        public string bank_code { get; set; }
        public string fullname { get; set; }
        public DateTime date_created { get; set; }
        public string currency { get; set; }
        public object debit_currency { get; set; }
        public double amount { get; set; }
        public int fee { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
        public object meta { get; set; }
        public string narration { get; set; }
        public object approver { get; set; }
        public string complete_message { get; set; }
        public int requires_approval { get; set; }
        public int is_approved { get; set; }
        public string bank_name { get; set; }
    }
}
