using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using System.ComponentModel;
using SonarUtils.Collections;
using System.Diagnostics.CodeAnalysis;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace Sonar.Messages
{
    // This class treat nulls in a special way
    // - They're not prevented from being added/inserted however its possible to have a null value.
    // - Null values are skipped during enumeration and deserialization.
    [MessagePackFormatter(typeof(MessageListFormatter))]
    public sealed class MessageList : IList<ISonarMessage>, ISonarMessage, ICloneable
    {
        private const int StartingCapacity = 8; // Common CPU L1 Cache line size: 64 bytes
        /// <summary>
        /// Default auto-flattening value (affects deserialization)
        /// </summary>
        public static bool DefaultAutoFlatten { get; set; } = true;
        
        /// <summary>
        /// Automatically flatten the list
        /// </summary>
        public bool AutoFlatten { get; set; } = DefaultAutoFlatten;
        private InternalList<ISonarMessage> list;
        
        public MessageList() : this(StartingCapacity) { }

        public MessageList(IEnumerable<ISonarMessage> source)
        {
            if (source is ICollection<ISonarMessage> collection) this.list = new(collection.Count);
            else this.list = new(StartingCapacity);
            this.AddRange(source);
        }

        public MessageList(int capacity)
        {
            this.list = new(capacity);
        }

        public ISonarMessage this[int index]
        {
            get => this.list[index];
            set => this.list[index] = value;
        }

        public int Count
        {
            get => this.list.Count;
            set => this.list.Resize(value, false);
        }

        public int Capacity
        {
            get => this.list.Capacity;
            set => this.list.Capacity = value;
        }

        public void Resize(int newSize) => this.list.Resize(newSize);
        public void Resize(int newSize, bool allowReduceCapacity) => this.list.Resize(newSize, allowReduceCapacity);
        public bool IsReadOnly => false;

        /// <summary>
        /// Create a MessageList from an IEnumerable
        /// </summary>
        public static MessageList CreateFrom<T>(IEnumerable<T> source) where T : ISonarMessage
        {
            var converted = new MessageList();
            converted.AddRange(source);
            return converted;
        }

        public void Add(ISonarMessage item)
        {
            if (item is null) return;
            if (this.AutoFlatten && item is MessageList list) this.AddRange(list);
            else this.list.Add(item);
        }

        public void AddRange<T>(IEnumerable<T> source) where T : ISonarMessage
        {
            foreach (var item in source) this.Add(item);
        }

        public void Clear()
        {
            this.list.Resize(0, false);
        }

        public void TrimExcess(bool force = false)
        {
            if (force) this.Capacity = this.Count;
            else this.list.Resize(this.Count, true);
        }

        public bool Contains(ISonarMessage item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(ISonarMessage[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ISonarMessage> GetEnumerator()
        {
            return this.list.AsEnumerable().Where(item => item is not null).GetEnumerator();
        }

        public int IndexOf(ISonarMessage item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, ISonarMessage item)
        {
            if (this.AutoFlatten && item is MessageList list) this.InsertRange(index, list);
            else this.list.Insert(index, item);
        }

        public void InsertRange<T>(int index, IEnumerable<T> source) where T : ISonarMessage
        {
            var reverseIndex = this.Count - index;
            if (reverseIndex == 0)
            {
                this.AddRange(source);
            }
            else if (source is ICollection<T> collection)
            {
                this.InsertRangeCore(index, collection);
            }
            else
            {
                // Slow and inefficient path
                foreach (var item in source) this.Insert(this.Count - reverseIndex, item);
            }
        }

        private int InsertRangeCore<T>(int index, ICollection<T> source) where T : ISonarMessage
        {
            if (index < 0 || index >= this.Count) throw new ArgumentOutOfRangeException(nameof(index));

            this.list.InsertRangeHelper(index, source.Count);
            var count = 0;
            foreach (var item in source)
            {
                if (this.AutoFlatten && item is MessageList list) count += this.InsertRangeCore(index + count, list);
                else this.list[index + count++] = item;
            }
            return count;
        }

        public bool Remove(ISonarMessage item)
        {
            return this.list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        public void RemoveAt(IEnumerable<int> indexes)
        {
            if (!indexes.Any()) return;
            var queue = new Queue<int>(indexes.Distinct().OrderBy(i => i));
            if (queue.First() < 0 || queue.Last() > this.Count) throw new ArgumentOutOfRangeException(nameof(indexes));
            int nextIndex = queue.Dequeue();

            int deleted = 0;
            var array = this.list.InternalArray;
            for (int index = 0; index < this.Count; index++)
            {
                if (index == nextIndex)
                {
                    deleted++;
                    queue.TryDequeue(out nextIndex); // Return needs to be unchecked
                }
                else array[index - deleted] = array[index];
            }
            this.list.Count -= deleted;
        }
        public void RemoveAt(params int[] indexes) => this.RemoveAt(indexes.AsEnumerable());

        public void RemoveRange(int index, int count)
        {
            this.list.RemoveRange(index, count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.AsEnumerable().Where(item => item is not null).GetEnumerator();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)] // Should never be needed
        public void RemoveNulls()
        {
            var nulls = 0;
            var array = this.list.InternalArray;
            for (var index = 0; index < this.Count; index++)
            {
                if (array[index] is null) nulls++;
                else array[index - nulls] = array[index];
            }
            this.list.Count -= nulls;
        }

        public MessageList Clone()
        {
            var clone = (MessageList)this.MemberwiseClone();
            clone.list = this.list.Clone();
            return clone;
        }

        object ICloneable.Clone() => this.Clone();

        public MessageList CloneAndTrim()
        {
            var clone = (MessageList)this.MemberwiseClone();
            clone.list = this.list.CloneAndTrim();
            return clone;
        }

        private sealed class MessageListFormatter : IMessagePackFormatter<MessageList>
        {
            public void Serialize(ref MessagePackWriter writer, MessageList? value, MessagePackSerializerOptions options)
            {
                if (value is null)  
                {
                    writer.WriteNil();
                    return;
                }

                var messageFormatter = options.Resolver.GetFormatter<ISonarMessage>()!;
                writer.WriteArrayHeader(value.Count);
                foreach (var item in value)
                {
                    messageFormatter.Serialize(ref writer, item, options);
                }
            }

            public MessageList Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil()) return null!;
                options.Security.DepthStep(ref reader);

                var messageFormatter = options.Resolver.GetFormatter<ISonarMessage>()!;
                var count = reader.ReadArrayHeader();
                var value = new MessageList();
                if (count > value.Capacity) value.Capacity = count;
                foreach (var _ in Enumerable.Range(0, count))
                {
                    value.Add(messageFormatter.Deserialize(ref reader, options));
                }
                reader.Depth--;
                return value;
            }
        }
    }
}
