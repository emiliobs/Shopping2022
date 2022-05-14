using Shopping2022.Common;
using Shopping2022.Models;

namespace Shopping2022.Helpers
{
    public interface IOrdersHelper
    {
        Task<Response> ProcessOrderAsync(ShowCartVIewModel showCartVIewModel);

        Task<Response> CancelOrderAsync(int id);

    } 
}
