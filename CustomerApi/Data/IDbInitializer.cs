using System;

namespace CustomerApi.Data
{
    public interface IDbInitializer : IAsyncDisposable, IDisposable
    {
        void Initialize();
    }
}
