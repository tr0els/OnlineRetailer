using System;
using System.Collections.Generic;
using System.Linq;
using OrderApi.Models;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Orders
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }

            List<Order> orders = new List<Order>
            {
                new Order {
                    CustomerId = 2,
                    Date = DateTime.Today,
                    OrderLines = new List<OrderLine>{
                        new OrderLine { ProductId = 1, Quantity = 2 } }
                },
                new Order
                {
                    CustomerId = 1,
                    Date = DateTime.Today,
                    OrderLines = new List<OrderLine>
                    {
                        new OrderLine { ProductId = 1, Quantity = 2 },
                        new OrderLine { ProductId = 2, Quantity = 1 }
                    }
                }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
