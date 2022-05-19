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
        public IQueryable<Order> GetOrders([Service] OrderApiContext context)
        {
            return context.Orders;
        }

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Order> GetOrdersByCustomer(int customerId, [Service] OrderApiContext context)
        {
            return context.Orders.Where(o => o.CustomerId == customerId);
        }
    }
}
