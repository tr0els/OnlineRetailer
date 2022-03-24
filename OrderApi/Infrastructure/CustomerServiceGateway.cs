using OrderApi.Dtos;
using RestSharp;

namespace OrderApi.Infrastructure
{
    public class CustomerServiceGateway : IServiceGateway<CustomerStatusDto>
    {
        string customerServiceBaseUrl;

        public CustomerServiceGateway(string baseUrl)
        {
            customerServiceBaseUrl = baseUrl;
        }

        public CustomerStatusDto Get(int id)
        {
            RestClient c = new RestClient(customerServiceBaseUrl);

            var request = new RestRequest("status/" + id.ToString());
            var response = c.GetAsync<CustomerStatusDto>(request);
            response.Wait();
            return response.Result;
        }
    }
}
