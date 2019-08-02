using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public abstract class SystemProvider : ISystemProvider
    {
        public abstract IEnumerable<string> Query(string path, object source = null);
    }
}