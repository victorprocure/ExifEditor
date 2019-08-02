using System.Collections.Generic;
using System.IO;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    internal sealed class LocalSystemProvider : SystemProvider
    {
        public override IEnumerable<string> Query(string path, object source = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                foreach (var i in DriveInfo.GetDrives())
                    yield return i.Name;
            }
            else
            {
                if (Directory.Exists(path))
                {
                    foreach (var i in Directory.EnumerateFileSystemEntries(path))
                        yield return i;
                }
            }
        }
    }
}