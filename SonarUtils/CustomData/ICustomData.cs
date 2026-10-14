using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SonarUtils.CustomData
{
    /// <summary>Interface allowing to attach custom data.</summary>
    public interface ICustomData
    {
        /// <summary>Custom data container for <see cref="ICustomData"/> interface.</summary>
        public ConcurrentDictionary<object, object?> CustomData { get; }
    }

    /// <summary>For easy copy-paste. The implementation below is expected to be copied and pasted into needed locations.</summary>
    file class ExampleImplementation : ICustomData
    {
        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1033")]
        ConcurrentDictionary<object, object?> ICustomData.CustomData { get => CustomDataUtils.FieldGetterImpl(ref field); } = default!;
    }
}
