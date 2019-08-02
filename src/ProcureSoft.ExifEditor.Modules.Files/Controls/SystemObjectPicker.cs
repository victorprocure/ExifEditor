using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public class StoragePicker : TreeView
    {
        public static readonly DependencyProperty CheckedPathsProperty = DependencyProperty.Register("CheckedPaths", typeof(IList<string>), typeof(StoragePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FileStyleProperty = DependencyProperty.Register("FileStyle", typeof(Style), typeof(StoragePicker),
            new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FileTemplateProperty = DependencyProperty.Register("FileTemplate", typeof(DataTemplate), typeof(StoragePicker),
            new FrameworkPropertyMetadata(default(DataTemplate), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FolderStyleProperty = DependencyProperty.Register("FolderStyle", typeof(Style), typeof(StoragePicker),
            new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FolderTemplateProperty = DependencyProperty.Register("FolderTemplate", typeof(DataTemplate), typeof(StoragePicker),
            new FrameworkPropertyMetadata(default(DataTemplate), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty QueryOnExpandedProperty = DependencyProperty.Register("QueryOnExpanded", typeof(bool), typeof(StoragePicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnQueryOnExpandedChanged));

        public static readonly DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof(string), typeof(StoragePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRootChanged));

        public static readonly DependencyProperty SystemObjectsProperty = DependencyProperty.Register("SystemObjects", typeof(CheckableSystemObjectCollection), typeof(StoragePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SystemProviderProperty = DependencyProperty.Register("SystemObjectProvider", typeof(ISystemProvider), typeof(StoragePicker),
                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSystemObjectProviderChanged));

        public StoragePicker()
        {
            SetCurrentValue(CheckedPathsProperty, new List<string>());
            SetCurrentValue(SystemObjectsProperty, new CheckableSystemObjectCollection());
            SetCurrentValue(SystemProviderProperty, new LocalSystemProvider());

            SetBinding(ItemsSourceProperty, new Binding()
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("SystemObjects", null),
                Source = this
            });

            SystemObjects.ItemStateChanged += OnSystemObjectStateChanged;
        }

        public IList<string> CheckedPaths
        {
            get => (IList<string>)GetValue(CheckedPathsProperty);
            private set => SetValue(CheckedPathsProperty, value);
        }

        public Style FileStyle
        {
            get => (Style)GetValue(FileStyleProperty);
            private set => SetValue(FileStyleProperty, value);
        }

        public DataTemplate FileTemplate
        {
            get => (DataTemplate)GetValue(FileTemplateProperty);
            private set => SetValue(FileTemplateProperty, value);
        }

        public Style FolderStyle
        {
            get => (Style)GetValue(FolderStyleProperty);
            private set => SetValue(FolderStyleProperty, value);
        }

        public DataTemplate FolderTemplate
        {
            get => (DataTemplate)GetValue(FolderTemplateProperty);
            private set => SetValue(FolderTemplateProperty, value);
        }

        public bool QueryOnExpanded
        {
            get => (bool)GetValue(QueryOnExpandedProperty);
            set => SetValue(QueryOnExpandedProperty, value);
        }

        public string Root
        {
            get => (string)GetValue(RootProperty);
            set => SetValue(RootProperty, value);
        }

        public ISystemProvider SystemObjectProvider
        {
            get => (ISystemProvider)GetValue(SystemProviderProperty);
            set => SetValue(SystemProviderProperty, value);
        }

        public CheckableSystemObjectCollection SystemObjects
        {
            get => (CheckableSystemObjectCollection)GetValue(SystemObjectsProperty);
            set => SetValue(SystemObjectsProperty, value);
        }

        protected virtual void OnQueryOnExpandedChanged(bool Value)
        {
            foreach (var i in SystemObjects)
                OnQueryOnExpandedChanged(i, Value);
        }

        protected virtual void OnRefreshed(ISystemProvider Provider, string Root)
        {
            SystemObjects.Clear();
            if (Provider != null)
            {
                foreach (var i in Provider.Query(Root))
                {
                    var j = new CheckableSystemObject(i, SystemObjectProvider)
                    {
                        QueryOnExpanded = QueryOnExpanded
                    };
                    SystemObjects.Add(j);
                }
            }
        }

        protected virtual void OnRootChanged(string Value) => OnRefreshed(SystemObjectProvider, Value);

        protected virtual void OnSystemObjectProviderChanged(ISystemProvider Value) => OnRefreshed(Value, Root);

        protected virtual void OnSystemObjectStateChanged(object sender, CheckedEventArgs e)
        {
            var i = sender as CheckableSystemObject;
            switch (e.State)
            {
                case true:
                    CheckedPaths.Add(i.Path);
                    break;

                case false:
                case null:
                    CheckedPaths.Remove(i.Path);
                    break;
            }
        }

        private static void OnQueryOnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as StoragePicker)?.OnQueryOnExpandedChanged((bool)e.NewValue);

        private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as StoragePicker)?.OnRootChanged((string)e.NewValue);

        private static void OnSystemObjectProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as StoragePicker)?.OnSystemObjectProviderChanged((ISystemProvider)e.NewValue);

        private void OnQueryOnExpandedChanged(CheckableSystemObject Item, bool Value)
        {
            foreach (var i in Item.Children)
            {
                i.QueryOnExpanded = Value;
                OnQueryOnExpandedChanged(i, Value);
            }
        }
    }
}