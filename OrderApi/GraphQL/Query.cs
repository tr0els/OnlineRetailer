using HotChocolate;
using HotChocolate.Data;
using OrderApi.Data;
using OrderApi.Models;
using System.Linq;

namespace OrderApi.GraphQL
{
    public class Query
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Order> GetOrder([Service] OrderApiContext context)
        {
            return context.Orders;
        }
    }
}
