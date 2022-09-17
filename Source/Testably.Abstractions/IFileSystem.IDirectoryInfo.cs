using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Models;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.IO.DirectoryInfo" />.
    /// </summary>
    public interface IDirectoryInfo
    {

    }
}