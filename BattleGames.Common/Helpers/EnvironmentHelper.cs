using System;

namespace Stip.BattleGames.Common.Helpers
{
    public class EnvironmentHelper : IInjectable
    {
        public virtual string ExecutableDirectory
            => AppContext.BaseDirectory;
    }
}
