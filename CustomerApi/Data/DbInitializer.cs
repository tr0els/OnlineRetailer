using CustomerApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Orders
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> customers = new List<Customer>
            {
                new Customer
                {
                    Name = "Jens Jensen",
                    Email = "jjensen@mockmail.com",
                    Phone = "12345678",
                    BillingAddress = "Nyvej 1",
                    ShippingAddress = "Gammelvej 1",
                    CreditStanding = 0
                },
                new Customer
                {
                    Name = "Hans Hansen",
                    Email = "hhansen@mockmail.com",
                    Phone = "87654321",
                    BillingAddress = "Østervænget 1",
                    ShippingAddress = "Vestervænget 1",
                    CreditStanding = -10000
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
