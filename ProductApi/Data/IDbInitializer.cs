using System;

namespace ProductApi.Data
{
    public interface IDbInitializer : IAsyncDisposable, IDisposable
    {
        void Initialize();
    }
}
