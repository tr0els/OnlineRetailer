using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        private ProductApiContext context;

        public DbInitializer(IDbContextFactory<ProductApiContext> dbContextFactory)
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

            // Look for any Products
            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            List<Product> products = new List<Product>
            {
                new Product { Name = "Hammer", Price = 100, ItemsInStock = 10, ItemsReserved = 0 },
                new Product { Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 0 },
                new Product { Name = "Drill", Price = 500, ItemsInStock = 2, ItemsReserved = 0 }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
