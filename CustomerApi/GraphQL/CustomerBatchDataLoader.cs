using CustomerApi.Data;
using CustomerApi.Models;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerApi.GraphQL
{
    public class CustomerBatchDataLoader : BatchDataLoader<int, Customer>
    {
        private readonly IDbContextFactory<CustomerApiContext> _dbContextFactory;

        public CustomerBatchDataLoader(
            IDbContextFactory<CustomerApiContext> dbContextFactory,
            IBatchScheduler batchScheduler, DataLoaderOptions options)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            // With await using, we will dispose context after it is no longer in use/required
            await using CustomerApiContext context = _dbContextFactory.CreateDbContext();

            // instead of fetching one customer, we fetch multiple customers
            return await context.Customers
                .Where(c => keys.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);
        }
    }
}
