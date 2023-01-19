﻿using System;
using System.Collections.Generic;

namespace Collections.Pooled
{
    /// <summary>
    /// Represents a read-only collection of pooled elements that can be accessed by index
    /// </summary>
    /// <typeparam name="T">The type of elements in the read-only pooled list.</typeparam>

    public interface IReadOnlyPooledList<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Gets a <see cref="ReadOnlySpan{T}"/> for the items currently in the collection.
        /// </summary>
        ReadOnlySpan<T> Span { get; }
        /// <summary>
        /// Gets a <see cref="ReadOnlyMemory{T}"/> for the items currently in the collection.
        /// </summary>
        ReadOnlyMemory<T> Memory { get; }
    }
}
