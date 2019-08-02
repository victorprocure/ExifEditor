using System.Windows;
using Prism.Ioc;
using Prism.Modularity;
using ProcureSoft.ExifEditor.Gui.Views;
using ProcureSoft.ExifEditor.Modules.Files;

namespace ProcureSoft.ExifEditor.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<FilesModule>();

        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}