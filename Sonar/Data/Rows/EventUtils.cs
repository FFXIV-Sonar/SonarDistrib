using Sonar.Data.Rows.Internal;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sonar.Data.Rows
{
    /// <summary>Utilities for <see cref="EventRow"/>.</summary>
    public static class EventUtils
    {
        // Intended id format (from most to least significant):
        // [Event Type (8 bits) .. RowId (rowBits) .. SubRowId (subRowBits)]
        // rowBits + subRowBits = 24 (+ event bits = 32)

        /// <summary>Contains the number of bits for the subrows for each <see cref="EventType"/>.</summary>
        private static readonly FrozenDictionary<EventType, int> s_subRowBits = Enum.GetValues<EventType>()
            .Select(value => KeyValuePair.Create(value, typeof(EventType).GetField(Enum.GetName(value)!)!.GetCustomAttribute<SubRowBitsAttribute>()))
            .Where(kvp => kvp.Value is not null).ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value!.Bits);

        /// <summary>Represents a <see cref="EventType"/>, row ID and subrow ID.</summary>
        /// <param name="Type"><see cref="EventType"/>.</param>
        /// <param name="RowId">Row ID.</param>
        /// <param name="SubRowId">Sub row ID.</param>
        public readonly record struct EventIdInfo(EventType Type, uint RowId, uint SubRowId)
        {
            /// <summary>Gets an ID representing this <see cref="EventIdInfo"/>.</summary>
            /// <returns>Packed ID.</returns>
            [SuppressMessage(null!, "S3218", Justification = "Intended.")]
            public uint ToId() => EventUtils.ToId(this.Type, this.RowId, this.SubRowId);
        }

        /// <summary>Gets an ID representing both the <paramref name="type"/>, <paramref name="rowId"/> and <paramref name="subRowId"/>.</summary>
        /// <param name="type"><see cref="EventType"/>.</param>
        /// <param name="rowId">Row ID.</param>
        /// <param name="subRowId">Sub row ID.</param>
        /// <returns>Packed ID.</returns>
        // /// <remarks><paramref name="subRowId"/> is only available if <see cref="SubRowBitsAttribute"/> has been applied to the respective <see cref="EventType"/> referenced by <paramref name="type"/> with <c>1</c> or greater number of bits.</remarks>
        public static uint ToId(EventType type, uint rowId, uint subRowId = 0)
        {
            s_subRowBits.TryGetValue(type, out var subRowBits); // ASSERT: subRowBits is 0 for Event Types without [SubRowBits]
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subRowId, 1u << subRowBits);

            var rowBits = 24 - subRowBits;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rowId, 1u << rowBits);

            return subRowId + (rowId << subRowBits) + ((uint)type << 24);
        }

        /// <summary>Extract the <see cref="EventType"/>, row ID and sub row ID from <paramref name="id"/>.</summary>
        /// <param name="id">ID to extract from.</param>
        /// <returns><see cref="EventIdInfo"/> containing the <see cref="EventType"/>, row ID and sub row ID from <paramref name="id"/>.</returns>
        public static EventIdInfo FromId(uint id)
        {
            var type = (EventType)(id >> 24);
            id &= 0x00ffffff; // Mask away the event type

            if (!s_subRowBits.TryGetValue(type, out var subRowBits)) subRowBits = 8;
            var subId = id & GetBitMask(subRowBits); // Mask away the row id
            id >>= subRowBits;

            return new(type, id, subId);
        }

        /// <summary>Extract the <see cref="EventType"/> from <paramref name="id"/>.</summary>
        /// <remarks>This is a reduced form of <see cref="FromId(uint)"/>.</remarks>
        /// <param name="id">ID to extract from.</param>
        /// <returns><see cref="EventType"/> extracted from <paramref name="id"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EventType TypeFromId(uint id) => (EventType)(id >> 24);

        /// <summary>Gets a bit mask for the least significant <paramref name="bits"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetBitMask(int bits) => (1u << bits) - 1;
    }
}
