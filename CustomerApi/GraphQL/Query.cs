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
        public IQueryable<Customer> GetCustomers([Service] CustomerApiContext context)
        {
            return context.Customers;
        }

        public Customer GetCustomer(int id, [Service] CustomerApiContext context)
        {
            return context.Customers.FirstOrDefault(c => c.Id == id);
        }
    }
}
