using Prism.Mvvm;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList.ViewModels
{
    public sealed class FileListViewModel : BindableBase
    {
        private IObservableFileList _fileList;

        public FileListViewModel(IObservableFileList fileList)
        {
            FileList = fileList;
        }

        public IObservableFileList FileList
        {
            get => _fileList;
            set => SetProperty(ref _fileList, value);
        }
    }
}