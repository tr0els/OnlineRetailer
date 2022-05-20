using System;

namespace OrderApi.Data
{
    public interface IDbInitializer : IAsyncDisposable, IDisposable
    {
        void Initialize();
    }
}
