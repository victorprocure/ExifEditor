using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ProcureSoft.ExifEditor.Infrastructure.Collections;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public sealed class CheckableSystemObjectCollection : ConcurrentCollectionImpl<CheckableSystemObject>
    {
        public event EventHandler<CheckedEventArgs> ItemStateChanged;

        protected override void OnItemAdded(CheckableSystemObject item)
        {
            base.OnItemAdded(item);
            item.StateChanged += OnItemStateChanged;
            item.Children.ItemStateChanged += OnItemStateChanged;
        }

        protected override void OnItemRemoved(CheckableSystemObject item)
        {
            base.OnItemRemoved(item);
            item.StateChanged -= OnItemStateChanged;
            item.Children.ItemStateChanged -= OnItemStateChanged;
        }

        protected override void OnPreviewItemsCleared()
        {
            base.OnPreviewItemsCleared();
            foreach (var i in this)
            {
                i.StateChanged -= OnItemStateChanged;
                i.Children.ItemStateChanged -= OnItemStateChanged;
            }
        }

        private void OnItemStateChanged(object sender, CheckedEventArgs e) => ItemStateChanged?.Invoke(sender as CheckableSystemObject, e);
    }
}