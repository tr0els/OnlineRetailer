using RestSharp;
using SharedModels;
using System.Collections.Generic;

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
            var result = response.Result;
            if (result.Id != 0)
            {
                return result;
            }
            throw new KeyNotFoundException($"No customer with ID: {id}");
            
        }
    }
}
