using Stip.Stipstonks.Helpers;
using System;

namespace Stip.Stipstonks.Factories
{
    public class PeriodicTimerFactory : IInjectable
    {
        public virtual IPeriodicTimer Create(TimeSpan period)
            => new PeriodicTimerWrapper(period);
    }
}
