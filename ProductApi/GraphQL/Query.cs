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
        public IQueryable<Product> GetProducts([Service] ProductApiContext context)
        {
            return context.Products;
        }
        
        public Product GetProduct(int id, [Service] ProductApiContext context)
        {
            return context.Products.FirstOrDefault(p => p.Id == id);
        }
    }
}
