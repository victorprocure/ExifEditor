using Prism.Mvvm;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList
{
    public sealed class FileListItem : BindableBase
    {
        private string _fileName;
        private string _filePath;
        private bool _hasExifData;

        internal FileListItem(string fileName, string filePath, bool hasExifData)
        {
            FileName = fileName;
            FilePath = filePath;
            HasExifData = hasExifData;
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public bool HasExifData
        {
            get => _hasExifData;
            set => SetProperty(ref _hasExifData, value);
        }
    }
}