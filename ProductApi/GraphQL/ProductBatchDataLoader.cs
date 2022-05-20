using GreenDonut;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductApi.GraphQL
{
    public class ProductBatchDataLoader : BatchDataLoader<int, Product>
    {
        private readonly IDbContextFactory<ProductApiContext> _dbContextFactory;

        public ProductBatchDataLoader(
            IDbContextFactory<ProductApiContext> dbContextFactory,
            IBatchScheduler batchScheduler, DataLoaderOptions options)
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, Product>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            // With await using, we will dispose context after it is no longer in use/required
            await using ProductApiContext context = _dbContextFactory.CreateDbContext();

            // instead of fetching one product, we fetch multiple products
            return await context.Products
                .Where(p => keys.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);
        }
    }
}