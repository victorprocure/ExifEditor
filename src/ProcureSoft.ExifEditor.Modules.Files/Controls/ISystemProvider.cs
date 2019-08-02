using System.Collections.Generic;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public interface ISystemProvider
    {
        IEnumerable<string> Query(string path, object source = null);
    }
}