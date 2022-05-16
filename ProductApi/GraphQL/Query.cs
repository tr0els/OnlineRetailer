using HotChocolate;
using HotChocolate.Data;
using ProductApi.Data;
using ProductApi.Models;
using System.Linq;

namespace ProductApi.GraphQL
{
    public class Query
    {
        [UseFiltering]
        [UseSorting]
        public IQueryable<Product> GetProduct([Service] ProductApiContext context)
        {
            return context.Products;
        }
    }
}
