using GreenDonut;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderApi.GraphQL
{
    public class OrderBatchDataLoader : BatchDataLoader<int, Order>
    {
        private readonly IDbContextFactory<OrderApiContext> _dbContextFactory;

        public OrderBatchDataLoader(
            IDbContextFactory<OrderApiContext> dbContextFactory,
            IBatchScheduler batchScheduler, DataLoaderOptions options)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, Order>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            // With await using, we will dispose context after it is no longer in use/required
            await using OrderApiContext context = _dbContextFactory.CreateDbContext();

            // instead of fetching one order, we fetch multiple orders
            return await context.Orders
                .Where(o => keys.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, cancellationToken);
        }
    }
}
