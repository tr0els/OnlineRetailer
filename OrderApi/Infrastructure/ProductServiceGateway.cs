using System;
using System.Collections.Generic;
using RestSharp;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class ProductServiceGateway : IServiceGateway<ProductDto>
    {
        string productServiceBaseUrl;

        public ProductServiceGateway(string baseUrl)
        {
            productServiceBaseUrl = baseUrl;
        }

        public ProductDto Get(int id)
        {
            RestClient c = new RestClient(productServiceBaseUrl);

            var request = new RestRequest(id.ToString());
            var response = c.GetAsync<ProductDto>(request);
            response.Wait();
            var result = response.Result;
            if (result.Id != 0)
            {
                return result;
            }
            throw new KeyNotFoundException($"No product with ID: {id}");
        }
    }
}
