using System.Collections.Generic;
using System.Linq;

namespace CustomerApi.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        IQueryable<T> GetAllAsQueryable();
        IQueryable<T> GetMany(IReadOnlyList<int> keys);
        T Get(int id);
        T Add(T entity);
        void Edit(T entity);
        void Remove(int id);
    }
}
