
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sonar.Utilities
{
    [SuppressMessage("Major Code Smell", "S4035", Justification = "Implemented on derived classes")]
    [SuppressMessage("Minor Code Smell", "S4136", Justification = "Generated code")]
    [SuppressMessage("Design", "CA1067", Justification = "Implemented on derived classes")]
    public abstract partial class StringKey : IEquatable<StringKey>, IEnumerable<uint>
    {

        public abstract int Length { get; }
        public abstract uint this[int index] { get; }

        public bool Equals(StringKey? other) => this.Equals(Unsafe.As<object>(other));

        public IEnumerator<uint> GetEnumerator()
        {
            for (var index = 0; index < this.Length; index++)
            {
                yield return this[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        
        public static StringKey Create(uint arg1)
        {

            return new Key1Parts(arg1);
            
        }

        private sealed class Key1Parts : StringKey
        {

            private readonly uint _key1;

            public Key1Parts(uint arg1)
            {

                this._key1 = arg1;

            }

            public override int Length => 1;

            public override uint this[int index]
            {
                get
                {
                
                    return (index + 1) switch
                    {

                        1 => this._key1,

                        _ => throw new ArgumentOutOfRangeException(nameof(index))

                    };

                }
            }


            public override int GetHashCode()
            {
            
                return HashCode.Combine(this._key1);
            
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                if (obj is not Key1Parts other) return false;
                return this._key1 == other._key1;
            }

            public override string ToString() => string.Join("_", this._key1);

        }

        
        public static StringKey Create(uint arg1, uint arg2)
        {

            return new Key2Parts(arg1, arg2);
            
        }

        private sealed class Key2Parts : StringKey
        {

            private readonly uint _key1;
            private readonly uint _key2;

            public Key2Parts(uint arg1, uint arg2)
            {

                this._key1 = arg1;
                this._key2 = arg2;

            }

            public override int Length => 2;

            public override uint this[int index]
            {
                get
                {
                
                    return (index + 1) switch
                    {

                        1 => this._key1,
                        2 => this._key2,

                        _ => throw new ArgumentOutOfRangeException(nameof(index))

                    };

                }
            }


            public override int GetHashCode()
            {
            
                return HashCode.Combine(this._key1, this._key2);
            
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                if (obj is not Key2Parts other) return false;
                return this._key1 == other._key1 && this._key2 == other._key2;
            }

            public override string ToString() => string.Join("_", this._key1, this._key2);

        }

        
        public static StringKey Create(uint arg1, uint arg2, uint arg3)
        {

            return new Key3Parts(arg1, arg2, arg3);
            
        }

        private sealed class Key3Parts : StringKey
        {

            private readonly uint _key1;
            private readonly uint _key2;
            private readonly uint _key3;

            public Key3Parts(uint arg1, uint arg2, uint arg3)
            {

                this._key1 = arg1;
                this._key2 = arg2;
                this._key3 = arg3;

            }

            public override int Length => 3;

            public override uint this[int index]
            {
                get
                {
                
                    return (index + 1) switch
                    {

                        1 => this._key1,
                        2 => this._key2,
                        3 => this._key3,

                        _ => throw new ArgumentOutOfRangeException(nameof(index))

                    };

                }
            }


            public override int GetHashCode()
            {
            
                return HashCode.Combine(this._key1, this._key2, this._key3);
            
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                if (obj is not Key3Parts other) return false;
                return this._key1 == other._key1 && this._key2 == other._key2 && this._key3 == other._key3;
            }

            public override string ToString() => string.Join("_", this._key1, this._key2, this._key3);

        }

        
        public static StringKey Create(uint arg1, uint arg2, uint arg3, uint arg4)
        {

            return new Key4Parts(arg1, arg2, arg3, arg4);
            
        }

        private sealed class Key4Parts : StringKey
        {

            private readonly uint _key1;
            private readonly uint _key2;
            private readonly uint _key3;
            private readonly uint _key4;

            public Key4Parts(uint arg1, uint arg2, uint arg3, uint arg4)
            {

                this._key1 = arg1;
                this._key2 = arg2;
                this._key3 = arg3;
                this._key4 = arg4;

            }

            public override int Length => 4;

            public override uint this[int index]
            {
                get
                {
                
                    return (index + 1) switch
                    {

                        1 => this._key1,
                        2 => this._key2,
                        3 => this._key3,
                        4 => this._key4,

                        _ => throw new ArgumentOutOfRangeException(nameof(index))

                    };

                }
            }


            public override int GetHashCode()
            {
            
                return HashCode.Combine(this._key1, this._key2, this._key3, this._key4);
            
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                if (obj is not Key4Parts other) return false;
                return this._key1 == other._key1 && this._key2 == other._key2 && this._key3 == other._key3 && this._key4 == other._key4;
            }

            public override string ToString() => string.Join("_", this._key1, this._key2, this._key3, this._key4);

        }

        
    }
}
