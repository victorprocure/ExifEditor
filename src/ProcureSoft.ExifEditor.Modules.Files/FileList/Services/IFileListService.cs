using System.Collections.Generic;
using System.IO;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList.Services
{
    public interface IFileListService
    {
        string CurrentDirectory { get; }

        void ChangeDirectory(string directory);

        IEnumerable<FileInfo> GetAllFiles();
    }
}