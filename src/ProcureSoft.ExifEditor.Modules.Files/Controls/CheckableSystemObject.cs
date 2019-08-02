using System;
using System.IO;
using System.Threading.Tasks;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public class CheckableSystemObject : CheckableObject, IDisposable
    {
        private readonly ISystemProvider _systemProvider;
        private CheckableSystemObjectCollection _children = new CheckableSystemObjectCollection();

        private bool _childrenLoaded;
        private bool _isExpanded;
        private bool _isSelected;
        private CheckableSystemObject _parent;

        private string _path;
        private bool _queryOnExpanded;
        private bool _stateChangeHandled;

        public CheckableSystemObject(string path, ISystemProvider systemProvider, bool? isChecked = false)
        {
            Path = path;
            _systemProvider = systemProvider;
            IsChecked = isChecked;

            if (File.Exists(path))
                return;

            if (Directory.Exists(path))
                _children.Add(new CheckableSystemObject());
        }

        private CheckableSystemObject()
        {
        }

        public event EventHandler Collapsed;

        public event EventHandler Expanded;

        public event EventHandler Selected;

        public CheckableSystemObjectCollection Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    if (value)
                        OnExpanded();
                    else
                        OnCollapsed();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value) && value)
                {
                    OnSelected(EventArgs.Empty);
                }
            }
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public bool QueryOnExpanded
        {
            get => _queryOnExpanded;
            set => SetProperty(ref _queryOnExpanded, value);
        }

        public async Task BeginQuery(ISystemProvider systemProvider) => await Task.Run(() => Query(systemProvider)).ConfigureAwait(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _children?.Dispose();
        }

        protected override void OnChecked()
        {
            base.OnChecked();

            if (_stateChangeHandled)
                return;

            foreach (var i in _children)
                i.IsChecked = false;

            Determine();
        }

        protected virtual void OnCollapsed() => Collapsed?.Invoke(this, EventArgs.Empty);

        protected virtual void OnExpanded()
        {
            Expanded?.Invoke(this, EventArgs.Empty);

            if (!_childrenLoaded || _queryOnExpanded)
            {
                _childrenLoaded = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BeginQuery(_systemProvider);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void Determine()
        {
            if (!(_parent is null))
            {
                _stateChangeHandled = true;
                var p = _parent;
                while (!(p is null))
                {
                    p.IsChecked = Determine(p);
                    p = p._parent;
                }
                _stateChangeHandled = false;
            }
        }

        private bool? Determine(CheckableSystemObject root)
        {
            var uniform = true;
            var result = default(bool?);

            var j = false;
            foreach (var i in root.Children)
            {
                if (!j)
                {
                    result = i.IsChecked;
                    j = true;
                }
                else if (result != i.IsChecked)
                {
                    uniform = false;
                    break;
                }
            }

            return !uniform ? null : result;
        }

        private void OnSelected(EventArgs e) => Selected?.Invoke(this, e);

        private void Query(ISystemProvider systemProvider)
        {
            Children.Clear();
            if (systemProvider is null)
                return;

            foreach (var i in systemProvider.Query(_path))
            {
                Children.Add(new CheckableSystemObject(i, systemProvider, isChecked)
                {
                    _parent = this
                });
            }
        }
    }
}