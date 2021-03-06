﻿/********************************************************
*                                                        *
*   © Copyright (C) Microsoft. All rights reserved.      *
*                                                        *
*********************************************************/

namespace Microsoft.VisualStudio.Threading
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an asynchronous method to release allocated resources.
    /// </summary>
    /// <remarks>
    /// Consider implementing <see cref="System.IAsyncDisposable"/> instead.
    /// </remarks>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        Task DisposeAsync();
    }
}
