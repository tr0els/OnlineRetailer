using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        private CustomerApiContext context;

        public DbInitializer(IDbContextFactory<CustomerApiContext> dbContextFactory)
        {
            context = dbContextFactory.CreateDbContext();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return context.DisposeAsync();
        }

        // This method will create and seed the database.
        public void Initialize()
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
                    CreditStanding = CreditStanding.Good
                },
                new Customer
                {
                    Name = "Hans Hansen",
                    Email = "hhansen@mockmail.com",
                    Phone = "87654321",
                    BillingAddress = "Østervænget 1",
                    ShippingAddress = "Vestervænget 1",
                    CreditStanding = CreditStanding.Bad
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
