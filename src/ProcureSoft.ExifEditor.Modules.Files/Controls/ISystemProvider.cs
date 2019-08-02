using System;
using System.Collections.Generic;
using System.Text;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public interface ISystemProvider
    {
        IEnumerable<string> Query(string path, object source = null);
    }
}