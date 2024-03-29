﻿using System;

namespace ProcureSoft.ExifEditor.Modules.Files.Controls
{
    public delegate void CheckedEventHandler(object sender, CheckedEventArgs e);

    public sealed class CheckedEventArgs : EventArgs
    {
        public CheckedEventArgs(bool? state) => State = state;

        public bool? State { get; }
    }
}