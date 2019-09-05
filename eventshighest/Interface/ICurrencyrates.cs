using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface ICurrency
    {
        Task Getexchangeratescurrencies();
        Task<dynamic> Getcurrencies();
        Task<bool> CheckcurrencyexistsAsync(string code);
    }
}
