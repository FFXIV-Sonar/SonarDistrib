using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class SonarHashing
    {
        private static readonly ConcurrentDictionary<int, string> s_lengthExceptionMessages = new();
        private static readonly ConcurrentBag<Sha256Digest> s_sha256Bag = [];

        // ASSERT: Digest sizes does not change
        public static readonly int Sha256DigestSize = new Sha256Digest().GetDigestSize();

        public static byte[] Sha256(ReadOnlySpan<byte> input)
        {
            var result = new byte[Sha256DigestSize];
            Sha256(input, result);
            return result;
        }

        public static void Sha256(ReadOnlySpan<byte> input, Span<byte> output)
        {
            EnsureOutputLength(output, Sha256DigestSize);
            var sha256 = GetOrCreateSha256Digest();
            sha256.BlockUpdate(input);
            sha256.DoFinal(output);
            s_sha256Bag.Add(sha256);
        }

        public static byte[] Sha256(Stream input)
        {
            var result = new byte[Sha256DigestSize];
            Sha256(input, result);
            return result;
        }

        public static void Sha256(Stream input, Memory<byte> output)
        {
            EnsureOutputLength(output.Span, Sha256DigestSize); // fail-fast
            var sha256 = GetOrCreateSha256Digest();
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                var result = 0;
                do
                {
                    sha256.BlockUpdate(buffer.AsSpan(0, result));
                    result = input.Read(buffer);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            sha256.DoFinal(output.Span);
            s_sha256Bag.Add(sha256);
        }

        public static async ValueTask<byte[]> Sha256Async(Stream input, CancellationToken cancellationToken = default)
        {
            var result = new byte[Sha256DigestSize];
            await Sha256Async(input, result, cancellationToken);
            return result;
        }

        public static async ValueTask Sha256Async(Stream input, Memory<byte> output, CancellationToken cancellationToken = default)
        {
            EnsureOutputLength(output.Span, Sha256DigestSize); // fail-fast
            var sha256 = GetOrCreateSha256Digest();
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                var result = 0;
                do
                {
                    sha256.BlockUpdate(buffer.AsSpan(0, result));
                    result = await input.ReadAsync(buffer, cancellationToken);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            sha256.DoFinal(output.Span);
            s_sha256Bag.Add(sha256);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Sha256Digest GetOrCreateSha256Digest()
        {
            if (!s_sha256Bag.TryTake(out var digest)) digest = new();
            return digest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureOutputLength(ReadOnlySpan<byte> output, int length)
        {
            if (output.Length < length) ThrowLengthArgumentException(length);
        }

        [DoesNotReturn]
        private static void ThrowLengthArgumentException(int length)
            => throw new ArgumentException(GetLengthArgumentExceptionMessage(length));

        private static string GetLengthArgumentExceptionMessage(int length) => s_lengthExceptionMessages.GetOrAdd(length, length => StringUtils.Intern($"output's length must be at least {length} bytes."));
    }
}
