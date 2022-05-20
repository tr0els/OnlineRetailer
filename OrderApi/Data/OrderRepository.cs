using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SharedModels;
using System;
using OrderApi.Models;
using System.Threading.Tasks;

namespace OrderApi.Data
{
    public class OrderRepository : IOrderRepository, IAsyncDisposable
    {
        private readonly OrderApiContext db;

        public OrderRepository(IDbContextFactory<OrderApiContext> dbContextFactory)
        {
            db = dbContextFactory.CreateDbContext();
        }

        public ValueTask DisposeAsync()
        {
            return db.DisposeAsync();
        }

        public Order Add(Order entity)
        {
            if (entity.Date == null)
                entity.Date = DateTime.Now;
            
            var newOrder = db.Orders.Add(entity).Entity;
            db.SaveChanges();
            return newOrder;
        }

        public void Edit(Order entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        public Order Get(int id)
        {
            return db.Orders.Include(o => o.OrderLines).FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Order> GetAll()
        {
            return db.Orders.ToList();
        }

        public IEnumerable<Order> GetByCustomer(int customerId)
        {
            var ordersForCustomer = from o in db.Orders
                                    where o.CustomerId == customerId
                                    select o;

            return ordersForCustomer.ToList();
        }

        public IEnumerable<Order> GetByProduct(int productId)
        {
            return db.Orders.Include(o => o.OrderLines)
                .Where(o => o.OrderLines.Any(ol => ol.ProductId == productId))
                .ToList();
        }

        public void Remove(int id)
        {
            var order = db.Orders.FirstOrDefault(p => p.Id == id);
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
