using System.Collections.Generic;
using System;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Data
{
    public class CustomerRepository : IRepository<Customer>, IAsyncDisposable
    {
        private readonly CustomerApiContext db;

        public CustomerRepository(IDbContextFactory<CustomerApiContext> dbContextFactory)
        {
            db = dbContextFactory.CreateDbContext();
        }
        public ValueTask DisposeAsync()
        {
            return db.DisposeAsync();
        }

        public Customer Add(Customer entity)
        {
            var newCustomer = db.Customers.Add(entity).Entity;
            db.SaveChanges();
            return newCustomer;
        }

        public void Edit(Customer entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        public Customer Get(int id)
        {
            return db.Customers.FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Customer> GetAll()
        {
            return db.Customers.ToList();
        }

        // works better with Hotchocolate filtering and sorting when we return IQueryable
        // alternatively, made try to use the decorators on methods here
        public IQueryable<Customer> GetAllAsQueryable()
        {
            return db.Customers;
        }

        public IQueryable<Customer> GetMany(IReadOnlyList<int> keys)
        {
            return db.Customers.Where(c => keys.Contains(c.Id));
        }

        public void Remove(int id)
        {
            var customer = db.Customers.FirstOrDefault(p => p.Id == id);
            db.Customers.Remove(customer);
            db.SaveChanges();
        }
    }
}
