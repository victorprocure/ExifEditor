using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ProcureSoft.ExifEditor.Infrastructure;
using ProcureSoft.ExifEditor.Modules.Files.FileList;
using ProcureSoft.ExifEditor.Modules.Files.FileList.Services;
using ProcureSoft.ExifEditor.Modules.Files.FileList.ViewModels;
using ProcureSoft.ExifEditor.Modules.Files.FileList.Views;

namespace ProcureSoft.ExifEditor.Modules.Files
{
    public sealed class FilesModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public FilesModule(IRegionManager regionManager) => _regionManager = regionManager;

        public void OnInitialized(IContainerProvider containerProvider)
            => _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => containerProvider.Resolve<FileListView>());

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IFileListService, FileListService>();
            containerRegistry.Register<IObservableFileList, ObservableFileList>();
            containerRegistry.Register<FileListViewModel>();
        }
    }
}