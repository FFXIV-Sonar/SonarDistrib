using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace Sonar.Services
{
    /// <summary>
    /// Random Service based on System.Random (Thread-safe)
    /// </summary>
    public sealed class RandomServiceTS : RandomServiceBase
    {
        private readonly Random randomSource;

        /// <summary>
        /// Construct a RandomService with the default seed
        /// </summary>
        public RandomServiceTS()
        {
            this.randomSource = new Random();
        }

        /// <summary>
        /// Construct a RandomService with the specified seed
        /// </summary>
        /// <param name="seed">Seed</param>
        public RandomServiceTS(int seed)
        {
            this.randomSource = new Random(seed);
        }

        /// <summary>
        /// Construct a RandomService with the specified Random object
        /// </summary>
        /// <param name="random">Random</param>
        public RandomServiceTS(Random random)
        {
            this.randomSource = random ?? new Random();
        }
        
        public override uint[] GetUints(int count)
        {
            uint[] ret = new uint[count];
            lock (this.randomSource)
            {
                for (int index = 0; index < count; index++)
                {
                    ret[index] = (uint)(this.randomSource.NextDouble() * u32Max);
                }
            }
            return ret;
        }

        public override ulong[] GetUlongs(int count)
        {
            ulong[] ret = new ulong[count];
            lock (this.randomSource)
            {
                for (int index = 0; index < count; index++)
                {
                    ret[index] = (uint)(this.randomSource.NextDouble() * u32Max);
                    ret[index] += (ulong)(this.randomSource.NextDouble() * u32Max) << 32;
                }
            }
            return ret;
        }
    }
}
