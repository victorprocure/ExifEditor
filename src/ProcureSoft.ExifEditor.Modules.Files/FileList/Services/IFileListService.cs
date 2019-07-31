using System.Collections.Generic;
using System.IO;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList.Services
{
    public interface IFileListService
    {
        string CurrentDirectory { get; }

        IEnumerable<FileInfo> GetAllFiles();

        void ChangeDirectory(string directory);
    }
}