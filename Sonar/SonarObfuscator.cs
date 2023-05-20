using System;
using System.Linq;

namespace Sonar
{
    /// <summary>Functions containing a simple obfuscator.</summary>
    internal static class SonarObfuscator
    {
        // Originally Obsuctate was Deobfuscate and vice-versa.
        // I swapped them to make obfuscation more complex than deobfuscation.
        // No benchmarks were performed

        /// <summary>Obfuscate bytes</summary>
        public static void Obfuscate(Span<byte> bytes)
        {
            var lastByte = (byte)0;
            foreach (ref var curByte in bytes)
            {
                var tmpByte = curByte;
                curByte ^= lastByte;
                lastByte = tmpByte;
            }
            bytes.Reverse();
            lastByte = 0;
            foreach (ref var curByte in bytes)
            {
                var tmpByte = curByte;
                curByte ^= lastByte;
                lastByte = tmpByte;
            }
        }

        /// <summary>Deobfuscate bytes</summary>
        public static void Deobfuscate(Span<byte> bytes)
        {
            var lastByte = (byte)0;
            foreach (ref var curByte in bytes)
            {
                curByte ^= lastByte;
                lastByte = curByte;
            }
            bytes.Reverse();
            lastByte = 0;
            foreach (ref var curByte in bytes)
            {
                curByte ^= lastByte;
                lastByte = curByte;
            }
        }

        public static void Test(Span<byte> original) // TODO: Move this to a tests project
        {
            var buffer = original.ToArray().AsSpan();
            Console.WriteLine($"Original bytes:\n{BitConverter.ToString(buffer.ToArray()).ToLowerInvariant().Replace("-", " ")}");

            Obfuscate(buffer);
            Console.WriteLine($"Obfuscated bytes:\n{BitConverter.ToString(buffer.ToArray()).ToLowerInvariant().Replace("-", " ")}");
            Console.WriteLine($"Test result: {(buffer.SequenceEqual(original) ? "FAIL" : "PASS")}");

            Deobfuscate(buffer);
            Console.WriteLine($"Deobfuscated bytes:\n{BitConverter.ToString(buffer.ToArray()).ToLowerInvariant().Replace("-", " ")}");
            Console.WriteLine($"Test result: {(buffer.SequenceEqual(original) ? "PASS" : "FAIL")}");
        }
    }
}
