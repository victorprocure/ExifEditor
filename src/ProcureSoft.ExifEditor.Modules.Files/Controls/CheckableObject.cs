using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Prism.Mvvm;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public class CheckableObject : BindableBase, ICheckable
    {
        protected bool? isChecked;

        public CheckableObject(bool isChecked = false) => IsChecked = isChecked;

        public event EventHandler<EventArgs> Checked;

        public event CheckedEventHandler StateChanged;

        public event EventHandler<EventArgs> Unchecked;

        public virtual bool? IsChecked
        {
            get => isChecked;
            set
            {
                if (SetProperty(ref isChecked, value) && value != null)
                {
                    if (value.Value)
                        OnChecked();
                    else
                        OnUnchecked();
                }
            }
        }

        public override string ToString() => isChecked?.ToString() ?? "Indeterminate";

        protected virtual void OnChecked()
        {
            Checked?.Invoke(this, EventArgs.Empty);
            OnStateChanged(true);
        }

        protected virtual void OnIndeterminate() => OnStateChanged(null);

        protected virtual void OnStateChanged(bool? State) => StateChanged?.Invoke(this, new CheckedEventArgs(State));

        protected virtual void OnUnchecked()
        {
            Unchecked?.Invoke(this, EventArgs.Empty);
            OnStateChanged(false);
        }
    }
}