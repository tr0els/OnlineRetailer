using CustomerApi.Data;
using CustomerApi.Models;
using GreenDonut;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerApi.GraphQL
{
    public class CustomerBatchRepositoryLoader : BatchDataLoader<int, Customer>
    {
        private readonly IRepository<Customer> _repository;

        public CustomerBatchRepositoryLoader(
            IRepository<Customer> repository,
            IBatchScheduler batchScheduler, DataLoaderOptions options)
            : base(batchScheduler, options)
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            // instead of fetching one customer, we fetch multiple customers
            var customers = _repository.GetMany(keys);
            // this would also work if repository converted with ToDictionary and we returned it immediately
            return customers.ToDictionary(c => c.Id);
        }
    }
}

