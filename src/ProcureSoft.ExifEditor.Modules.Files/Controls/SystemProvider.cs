using System.Collections.Generic;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public abstract class SystemProvider : ISystemProvider
    {
        public abstract IEnumerable<string> Query(string path, object source = null);
    }
}