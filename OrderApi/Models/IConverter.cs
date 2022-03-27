using System.Collections.Generic;

namespace OrderApi.Models
{
    public interface IConverter<T,U>
    {
        T Convert(U model);
        U Convert(T model);
        IList<T> ConvertMany(IList<U> models);
        IList<U> ConvertMany(IList<T> models);
    }
}
