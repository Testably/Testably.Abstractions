﻿using System;
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
        ICallbackHandler ChangeOccurring(Action<FileSystemChange> callback,
                                         Func<FileSystemChange, bool>? predicate = null);

        /// <summary>
        ///     Callback executed when any change in the <see cref="FileSystemMock" /> occurred.
        ///     <para />
        ///     Returns an <see cref="IDisposable" /> to un-register the callback.
        /// </summary>
        Notification.IAwaitableCallback<FileSystemChange> ChangeOccurred(
            Action<FileSystemChange>? callback = null,
            Func<FileSystemChange, bool>? predicate = null);

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
            public CallbackChangeType Type { get; }

            /// <summary>
            ///     The property changes affected by the change.
            /// </summary>
            public NotifyFilters NotifyFilters { get; }

            internal FileSystemChange(string path, CallbackChangeType type,
                                      NotifyFilters notifyFilters)
            {
                Path = path;
                Type = type;
                NotifyFilters = notifyFilters;
            }

            /// <inheritdoc cref="object.ToString()" />
            public override string ToString()
            {
                return $"{Type} {Path} [{NotifyFilters}]";
            }
        }
    }
}