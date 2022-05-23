using CustomerApi.Data;
using CustomerApi.Models;
using HotChocolate;
using HotChocolate.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.GraphQL
{
    public class Query
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        // UseProjection will not only get nested objects, but will also select only those fields from database
        // that we ask for in query operation; it can only be used with IQueryable return values
        public IQueryable<Customer> GetCustomers(CustomerApiContext context)
        {
            return context.Customers;
        }

        [UseFiltering]
        [UseSorting]
        // if repository converts IQueryable to List, and this method converts it back to
        // IQueryable again, sorting and filtering still works, but the select statement will still just grab
        // everything from context
        // If repository instead returns the IQueryable directly, logging shows that select statement includes a where
        // or order statement dependending on our query!
        public IQueryable<Customer> GetCustomersFromRepository(IRepository<Customer> repo)
        {
            return repo.GetAllAsQueryable();
        }

        public Customer GetCustomer(int id, CustomerApiContext context)
        {
            return context.Customers.FirstOrDefault(c => c.Id == id);
        }

        // this way we can also have projection on GetById type resolvers
        [UseFirstOrDefault]
        [UseProjection]
        public IQueryable<Customer> GetCustomerWithProjection(int id, CustomerApiContext context)
        {
            return context.Customers.Where(c => c.Id == id);
        }

        public async Task<Customer> GetCustomerFromDataloader(int id, CustomerBatchDataLoader loader)
        {
            return await loader.LoadAsync(id);
        }

        public async Task<Customer> GetCustomerFromRepositoryLoader(int id, CustomerBatchRepositoryLoader loader)
        {
            return await loader.LoadAsync(id);
        }
    }
}
