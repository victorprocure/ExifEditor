using System;
using System.Collections.ObjectModel;
using System.IO;
using Prism.Events;
using ProcureSoft.ExifEditor.Infrastructure;
using ProcureSoft.ExifEditor.Modules.Files.FileList.Services;
using TagLib;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList
{
    public sealed class ObservableFileList : IObservableFileList, IDisposable
    {
        private readonly IFileListService _fileListService;
        private readonly SubscriptionToken _fileDirectoryChangedEventToken;

        public ObservableFileList(IFileListService fileListService, IEventAggregator eventAggregator)
        {
            _fileListService = fileListService;

            _fileDirectoryChangedEventToken = eventAggregator.GetEvent<FileDirectoryChangedEvent>().Subscribe(FileDirectoryChanged);
            PopulateFileList();
        }

        public ObservableCollection<FileListItem> Files { get; } = new ObservableCollection<FileListItem>();

        public void Dispose() => _fileDirectoryChangedEventToken.Dispose();

        private void FileDirectoryChanged((string oldFileDirectory, string newFileDirectory) fileDirectoryChangedEventArgs)
        {
            if (string.Equals(fileDirectoryChangedEventArgs.oldFileDirectory,
                fileDirectoryChangedEventArgs.newFileDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            PopulateFileList();
        }

        private void PopulateFileList()
        {
            Files.Clear();
            foreach (var file in _fileListService.GetAllFiles())
            {
                var fileListItem = new FileListItem(file.Name, file.Directory.FullName, HasExifData(file));
                Files.Add(fileListItem);
            }
        }

        private bool HasExifData(FileInfo fileInfo)
        {
            try
            {
                using var tagFile = TagLib.File.Create(fileInfo.FullName);
                return tagFile.Tag is TagLib.Image.CombinedImageTag tag;
            }
            catch (UnsupportedFormatException)
            {
                return false;
            }
        }
    }
}