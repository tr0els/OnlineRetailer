using CustomerApi.Data;
using CustomerApi.Models;
using HotChocolate;
using HotChocolate.Data;
using System.Linq;

namespace CustomerApi.GraphQL
{
    public class Query
    {
        [UseFiltering]
        [UseSorting]
        public IQueryable<Customer> GetCustomer([Service] CustomerApiContext context)
        {
            return context.Customers;
        }
    }
}
