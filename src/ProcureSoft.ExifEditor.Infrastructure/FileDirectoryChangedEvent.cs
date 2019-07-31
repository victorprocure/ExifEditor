using Prism.Events;

namespace ProcureSoft.ExifEditor.Infrastructure
{
    public sealed class FileDirectoryChangedEvent : PubSubEvent<(string oldFileDirectory, string newFileDirectory)>
    {
    }
}