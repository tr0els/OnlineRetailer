using HotChocolate;
using HotChocolate.Data;
using OrderApi.Data;
using OrderApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OrderApi.GraphQL
{
    public class Query
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Order> GetOrders(OrderApiContext context)
        {
            return context.Orders;
        }

        public async Task<Order> GetOrder(int id, OrderBatchDataLoader loader)
        {
            return await loader.LoadAsync(id);
        }

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Order> GetOrdersByCustomer(int customerId, OrderApiContext context)
        {
            return context.Orders.Where(o => o.CustomerId == customerId);
        }
    }
}
