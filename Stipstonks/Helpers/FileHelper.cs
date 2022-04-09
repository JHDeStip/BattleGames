using Stip.Stipstonks.Common;
using System.IO;

namespace Stip.Stipstonks.Helpers
{
    public class FileHelper : IInjectable
    {
        public virtual ActionResult<Stream> OpenStream(string path, FileMode mode)
        {
            try
            {
                return new ActionResult<Stream>(File.Open(path, mode));
            }
            catch
            {
                return ActionResult<Stream>.Failure;
            }
        }
    }
}
