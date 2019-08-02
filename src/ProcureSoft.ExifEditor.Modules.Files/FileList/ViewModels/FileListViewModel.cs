using System.Collections;
using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList.ViewModels
{
    public sealed class FileListViewModel : BindableBase
    {
        private readonly ObservableCollection<FileListItem> _selectedItems = new ObservableCollection<FileListItem>();
        private IObservableFileList _fileList;

        public FileListViewModel(IObservableFileList fileList) => FileList = fileList;

        public IObservableFileList FileList
        {
            get => _fileList;
            set => SetProperty(ref _fileList, value);
        }

        public IList SelectedItems
        {
            get => _selectedItems;
#pragma warning disable S4275 // Getters and setters should access the expected fields
            set
#pragma warning restore S4275 // Getters and setters should access the expected fields
            {
                _selectedItems.Clear();
                foreach (FileListItem item in value)
                    _selectedItems.Add(item);
            }
        }
    }
}