using System;

namespace Stip.Stipstonks.Helpers
{
    public class EnvironmentHelper : IInjectable
    {
        public virtual string ExecutableDirectory
            => AppContext.BaseDirectory;
    }
}
