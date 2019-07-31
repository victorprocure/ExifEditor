using System.Collections.ObjectModel;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList
{
    public interface IObservableFileList
    {
        ObservableCollection<FileListItem> Files { get; }
    }
}