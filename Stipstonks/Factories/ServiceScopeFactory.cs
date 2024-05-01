using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Factories
{
    public class ServiceScopeFactory
        (IServiceScopeFactory _serviceScopeFactory)
        : IInjectable
    {
        public class AsyncServiceScopeWrapper(
            AsyncServiceScope _asyncServiceScope)
            : IAsyncDisposable
        {
            public virtual T GetRequiredService<T>()
                => _asyncServiceScope.ServiceProvider.GetRequiredService<T>();

            public virtual ValueTask DisposeAsync()
                => _asyncServiceScope.DisposeAsync();
        }

        public virtual AsyncServiceScopeWrapper CreateAsyncScope()
            => new(_serviceScopeFactory.CreateAsyncScope());

        public virtual IServiceScope CreateScope()
            => _serviceScopeFactory.CreateScope();
    }
}
