using Prism.Mvvm;

namespace ProcureSoft.ExifEditor.Gui.ViewModels
{
    public sealed class MainWindowViewModel : BindableBase
    {
        private string _title = "Exif Editor";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}