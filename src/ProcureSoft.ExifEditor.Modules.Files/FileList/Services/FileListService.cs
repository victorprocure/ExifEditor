using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prism.Events;
using ProcureSoft.ExifEditor.Infrastructure;

namespace ProcureSoft.ExifEditor.Modules.Files.FileList.Services
{
    public sealed class FileListService : IFileListService
    {
        private readonly IEventAggregator _eventAggregator;

        public FileListService(IEventAggregator eventAggregator) => _eventAggregator = eventAggregator;

        public string CurrentDirectory { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        public void ChangeDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Unable to navigate to directory: {directory}, NOT FOUND");

            var oldDirectory = CurrentDirectory;
            CurrentDirectory = directory;

            _eventAggregator.GetEvent<FileDirectoryChangedEvent>().Publish((oldDirectory, CurrentDirectory));
        }

        public IEnumerable<FileInfo> GetAllFiles()
                    => Directory.GetFiles(CurrentDirectory).Select(f => new FileInfo(f));
    }
}