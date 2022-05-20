using HotChocolate;
using HotChocolate.Data;
using ProductApi.Data;
using ProductApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.GraphQL
{
    public class Query
    {
        [UseFiltering]
        [UseSorting]
        public IQueryable<Product> GetProducts(ProductApiContext context)
        {
            return context.Products;
        }
        
        public async Task<Product> GetProduct(int id, ProductBatchDataLoader loader)
        {
            return await loader.LoadAsync(id);
        }
    }
}
