using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ProcureSoft.ExifEditor.Gui.Controls
{
    internal sealed class InformationControl : ItemsControl
    {
        public static readonly DependencyProperty HeadersProperty =
            DependencyProperty.Register("Headers", typeof(ObservableCollection<object>), typeof(InformationControl), null);

        public InformationControl() => Headers = new ObservableCollection<object>();

        public ObservableCollection<object> Headers
        {
            get => (ObservableCollection<object>)GetValue(HeadersProperty);
            set => SetValue(HeadersProperty, value);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            base.OnItemsChanged(e);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems[0];
                    var header = GetHeader(newItem as FrameworkElement);
                    Headers.Insert(e.NewStartingIndex, header);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Headers.RemoveAt(e.OldStartingIndex);
                    break;
            }
        }

        private static DependencyObject GetHeader(FrameworkElement view)
            => view?.Resources["HeaderIcon"] is DataTemplate template ? template.LoadContent() : null;
    }
}