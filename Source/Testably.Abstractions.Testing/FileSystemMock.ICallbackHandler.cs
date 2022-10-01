using System;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The callback handler for the <see cref="FileSystemMock" />
    /// </summary>
    public interface ICallbackHandler
    {
        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> is about to occur.
        ///     <para />
        ///     This allows e.g. to throw custom exceptions instead.
        /// </summary>
        ICallbackHandler ChangeOccurring(Action<FileSystemChange> callback);

        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> occurred.
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        Notification.IAwaitableCallback<FileSystemChange> ChangeOccurred(
            Action<FileSystemChange>? callback = null);

        /// <summary>
        ///     Describes the change in the <see cref="FileSystemMock" />.
        /// </summary>
        public class FileSystemChange
        {
            /// <summary>
            ///     The path of the file or directory that changed.
            /// </summary>
            public string Path { get; }

            /// <summary>
            ///     The type of the change.
            /// </summary>
            public ChangeType Type { get; }

            /// <summary>
            ///     The property changes affected by the change.
            /// </summary>
            public NotifyFilters NotifyFilters { get; }

            internal FileSystemChange(string path, ChangeType type,
                                      NotifyFilters notifyFilters)
            {
                Path = path;
                Type = type;
                NotifyFilters = notifyFilters;
            }
        }

        /// <summary>
        ///     The type of the change in the <see cref="FileSystemMock" />
        /// </summary>
        public enum ChangeType
        {
            /// <summary>
            ///     The file or directory is created.
            /// </summary>
            Created,

            /// <summary>
            ///     The file or directory is removed.
            /// </summary>
            Deleted,

            /// <summary>
            ///     The file or directory is modified.
            /// </summary>
            Modified,

            /// <summary>
            ///     The file or directory is renamed.
            /// </summary>
            Renamed
        }
    }
}