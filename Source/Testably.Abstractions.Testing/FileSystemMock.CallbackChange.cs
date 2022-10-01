using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     Describes the change in the <see cref="FileSystemMock" />.
    /// </summary>
    public class CallbackChange
    {
        /// <summary>
        ///     The path of the file or directory that changed.
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     The type of the change.
        /// </summary>
        public CallbackChangeTypes Type { get; }

        /// <summary>
        ///     The property changes affected by the change.
        /// </summary>
        public NotifyFilters NotifyFilters { get; }

        internal CallbackChange(string path, CallbackChangeTypes type,
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